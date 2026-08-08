// Opens a print-ready A4 work order (작업지시서) in a new window and triggers
// the browser print dialog. Kept framework-free so it can be called from any
// component with the order + prepared item rows.

import QRCode from 'qrcode';

// Generated locally instead of via an external HTTP barcode API — avoids
// blank barcodes / premature print-window closes on slow connections.
async function generateQrDataUrl(text: string): Promise<string> {
  try {
    return await QRCode.toDataURL(text || '-', { width: 150, margin: 1 });
  } catch (error) {
    console.error('Failed to generate QR code:', error);
    return '';
  }
}

interface WorkOrderPrintData {
  orderNo?: string;
  orderDate?: string;
  logisticsCompanyName?: string;
  manufacturerName?: string;
  factoryRemarks?: string;
  workOrderRemarks?: string;
}

export async function printWorkOrder(order: WorkOrderPrintData, items: any[], codeMap: Record<string, string> = {}) {
  const win = window.open('', '_blank');
  if (!win) return;

  const esc = (v: any) => String(v ?? '').replace(/[&<>"]/g, (c) => (
    { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c] as string
  ));
  const codeName = (code: string) => (code && codeMap[code]) || code || '';
  const now = new Date();
  const pad = (n: number) => String(n).padStart(2, '0');
  const issueDate = `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())} ${pad(now.getHours())}:${pad(now.getMinutes())}`;

  const manufacturer = order.manufacturerName
    || items.find((i) => i.manufacturerName)?.manufacturerName
    || '-';

  const barcodeValues = items.map((item) => `${order.orderNo || ''}-${item.orderItemId ?? ''}`);
  const barcodeUrls = await Promise.all(barcodeValues.map((v) => generateQrDataUrl(v)));

  let seq = 0;
  const rowsHtml = items.map((item, idx) => {
    const isChild = !!item.isChild;
    if (!isChild) seq += 1;
    const name = item.productName || item.productSetTitle || '-';
    const opts = [
      codeName(item.purity),
      item.color && item.color !== 'EMPTY' ? codeName(item.color) : '',
      item.size && item.size !== 'EMPTY' ? codeName(item.size) : ''
    ].filter(Boolean).join(' / ') || '-';
    const reqWeight = item.requestedWeight ? `${item.requestedWeight}g` : '-';
    const memo = [item.memo, item.requestedMemo || item.inspectionMemo].filter(Boolean).join(' / ');
    const asBadge = item.isAsOrder ? '<span class="as">AS</span>' : '';
    const setBadge = item.isSet ? '<span class="set">SET</span>' : '';
    const barcodeValue = barcodeValues[idx];
    const barcodeUrl = barcodeUrls[idx];

    return `
      <tr class="${isChild ? 'child' : ''}">
        <td class="c">${isChild ? '&#8627;' : seq}</td>
        <td class="c barcode-cell">
          <img class="barcode-img" src="${barcodeUrl}" />
          <div class="barcode-no">${esc(barcodeValue)}</div>
        </td>
        <td>
          <div class="pname">${setBadge}${esc(name)} ${asBadge}</div>
          <div class="pno"><b>No:</b> ${esc(item.productNo || '-')}</div>
          ${item.size && item.size !== 'EMPTY' ? `<div class="pno"><b>사이즈:</b> ${esc(item.size)}</div>` : ''}
          <div class="mfg">${esc(item.manufacturerName || '')}</div>
        </td>
        <td class="c qty">${esc(item.quantity ?? '-')}</td>
        <td class="c">${esc(opts)}</td>
        <td class="c strong">${esc(reqWeight)}</td>
        <td class="memo">${esc(memo)}</td>
      </tr>`;
  }).join('');

  win.document.write(`
    <html>
      <head>
        <title>작업지시서 - ${esc(order.orderNo || '')}</title>
        <style>
          @page { size: A4; margin: 14mm; }
          * { box-sizing: border-box; }
          body {
            margin: 0;
            font-family: 'Malgun Gothic', 'Apple SD Gothic Neo', sans-serif;
            color: #1a1a1a;
            font-size: 12px;
          }
          .doc-title {
            text-align: center;
            font-size: 26px;
            font-weight: 800;
            letter-spacing: 12px;
            padding: 4px 0 2px 12px;
            border-bottom: 3px solid #1a1a1a;
            margin-bottom: 14px;
          }
          .meta {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 4px 24px;
            margin-bottom: 16px;
          }
          .meta .row { display: flex; gap: 8px; }
          .meta .k { min-width: 64px; font-weight: 700; color: #555; }
          .meta .v { flex: 1; border-bottom: 1px solid #ddd; padding-bottom: 1px; }
          table { width: 100%; border-collapse: collapse; }
          thead th {
            background: #f2f0eb;
            border: 1px solid #b9b5ac;
            padding: 7px 6px;
            font-size: 11px;
            font-weight: 700;
          }
          tbody td {
            border: 1px solid #cfccc4;
            padding: 7px 8px;
            vertical-align: middle;
          }
          tbody tr.child td { background: #fbfaf7; }
          td.c { text-align: center; }
          td.qty { font-weight: 700; }
          td.strong { font-weight: 700; color: #8a6d3b; }
          .barcode-cell { padding: 4px !important; }
          .barcode-img { width: 16mm; height: 16mm; object-fit: contain; }
          .barcode-no { font-size: 7px; color: #666; word-break: break-all; margin-top: 1px; }
          .pname { font-weight: 700; font-size: 12.5px; }
          .pno { color: #666; font-size: 10.5px; margin-top: 1px; }
          .mfg { color: #b07d2b; font-size: 10.5px; margin-top: 1px; }
          .memo { font-size: 11px; color: #333; }
          .as {
            display: inline-block; background: #f56c6c; color: #fff;
            font-size: 9px; font-weight: 700; padding: 0 4px; border-radius: 3px; margin-left: 4px;
          }
          .set {
            display: inline-block; background: #e6a23c; color: #fff;
            font-size: 9px; font-weight: 700; padding: 0 4px; border-radius: 3px; margin-right: 4px;
          }
          .remarks { margin-top: 16px; display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
          .remark-box { border: 1px solid #cfccc4; border-radius: 3px; min-height: 70px; }
          .remark-box .h {
            background: #f2f0eb; border-bottom: 1px solid #cfccc4;
            padding: 5px 8px; font-weight: 700; font-size: 11px;
          }
          .remark-box .b { padding: 8px; white-space: pre-wrap; line-height: 1.5; }
          .signoff {
            margin-top: 22px; display: flex; justify-content: flex-end; gap: 36px;
            font-size: 12px;
          }
          .signoff .cell { display: flex; align-items: flex-end; gap: 8px; }
          .signoff .cell .k { font-weight: 700; color: #555; }
          .signoff .cell .line { display: inline-block; width: 90px; border-bottom: 1px solid #333; }
        </style>
      </head>
      <body>
        <div class="doc-title">작 업 지 시 서</div>

        <div class="meta">
          <div class="row"><span class="k">주문번호</span><span class="v">${esc(order.orderNo || '-')}</span></div>
          <div class="row"><span class="k">발행일시</span><span class="v">${issueDate}</span></div>
          <div class="row"><span class="k">물류센터</span><span class="v">${esc(order.logisticsCompanyName || '-')}</span></div>
          <div class="row"><span class="k">제조사</span><span class="v">${esc(manufacturer)}</span></div>
        </div>

        <table>
          <thead>
            <tr>
              <th style="width: 34px;">No</th>
              <th style="width: 64px;">바코드</th>
              <th>제품 정보</th>
              <th style="width: 48px;">수량</th>
              <th style="width: 90px;">옵션</th>
              <th style="width: 78px;">의뢰중량</th>
              <th style="width: 150px;">메모</th>
            </tr>
          </thead>
          <tbody>
            ${rowsHtml || '<tr><td colspan="7" class="c">항목이 없습니다.</td></tr>'}
          </tbody>
        </table>

        <div class="remarks">
          <div class="remark-box">
            <div class="h">물류 요청사항</div>
            <div class="b">${esc(order.factoryRemarks || '')}</div>
          </div>
          <div class="remark-box">
            <div class="h">작업지시 메모</div>
            <div class="b">${esc(order.workOrderRemarks || '')}</div>
          </div>
        </div>

        <div class="signoff">
          <div class="cell"><span class="k">작업자</span><span class="line"></span></div>
          <div class="cell"><span class="k">검수</span><span class="line"></span></div>
          <div class="cell"><span class="k">일자</span><span class="line"></span></div>
        </div>

        <script>
          const imgs = document.querySelectorAll('img.barcode-img');
          let loadedCount = 0;
          const totalImgs = imgs.length;

          const triggerPrint = () => {
            window.print();
            window.close();
          };

          if (totalImgs === 0) {
            triggerPrint();
          } else {
            imgs.forEach(img => {
              if (img.complete) {
                loadedCount++;
                if (loadedCount === totalImgs) triggerPrint();
              } else {
                img.onload = () => {
                  loadedCount++;
                  if (loadedCount === totalImgs) triggerPrint();
                };
                img.onerror = () => {
                  loadedCount++;
                  if (loadedCount === totalImgs) triggerPrint();
                };
              }
            });
          }
        <\/script>
      </body>
    </html>
  `);
  win.document.close();
}

// One row per order (using its primary/first item), for a printable batch list of
// currently factory-approved orders - a picking/production list, distinct from the
// per-order 작업지시서 above.
export function printApprovedOrdersList(orders: any[], codeMap: Record<string, string> = {}) {
  const win = window.open('', '_blank');
  if (!win) return;

  const esc = (v: any) => String(v ?? '').replace(/[&<>"]/g, (c) => (
    { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c] as string
  ));
  const codeName = (code: string) => (code && codeMap[code]) || code || '';

  const formatOrderDate = (d?: string) => {
    if (!d) return '-';
    const date = new Date(d);
    if (isNaN(date.getTime())) return d;
    const pad = (n: number) => String(n).padStart(2, '0');
    const hours = date.getHours();
    const ampm = hours < 12 ? '오전' : '오후';
    const h12 = hours % 12 === 0 ? 12 : hours % 12;
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())} ${ampm} ${h12}:${pad(date.getMinutes())}:${pad(date.getSeconds())}`;
  };

  const rowsHtml = orders.map((order) => {
    const item = (order.orderItems || []).find((i: any) => !i.parentId) || (order.orderItems || [])[0] || {};
    const name = item.productName || item.productSetTitle || '-';
    const size = item.size && item.size !== 'EMPTY' ? item.size : '';
    const weight = item.requestedWeight || item.actualWeight || 0;

    return `
      <tr>
        <td class="c">${esc(order.orderNo || order.id || '-')}</td>
        <td class="goods">
          <img class="thumb" src="${esc(item.photoUrl || '/thumb_no_img.png')}" />
          <div class="goods-text">
            <div class="gname">${esc(name)}</div>
            ${size ? `<div class="gsize">표준 사이즈 : ${esc(size)}</div>` : ''}
          </div>
        </td>
        <td class="detail">
          <div>주문정보(생산): ${esc(item.manufacturerName || '-')}</div>
          <div>주문정보(물류): ${esc(order.logisticsCompanyName || '-')}</div>
        </td>
        <td class="c">${esc(codeName(item.purity) || '-')}</td>
        <td class="c">${weight ? `${weight}g` : '-'}</td>
        <td class="c">${esc(item.quantity ?? '-')}개</td>
        <td class="c date">${esc(formatOrderDate(order.createdAt))}</td>
      </tr>`;
  }).join('');

  win.document.write(`
    <html>
      <head>
        <title>주문 목록</title>
        <style>
          @page { size: A4; margin: 14mm; }
          * { box-sizing: border-box; }
          body { margin: 0; font-family: 'Malgun Gothic', 'Apple SD Gothic Neo', sans-serif; color: #1a1a1a; font-size: 12px; }
          .doc-title { font-size: 22px; font-weight: 800; margin-bottom: 14px; }
          table { width: 100%; border-collapse: collapse; }
          thead th { background: #f2f0eb; border: 1px solid #b9b5ac; padding: 8px 6px; font-size: 11px; font-weight: 700; }
          tbody td { border: 1px solid #cfccc4; padding: 8px; vertical-align: middle; }
          td.c { text-align: center; }
          td.date { font-size: 11px; color: #555; }
          .goods { display: flex; align-items: center; gap: 8px; }
          .thumb { width: 44px; height: 44px; object-fit: cover; border-radius: 3px; border: 1px solid #ddd; flex-shrink: 0; }
          .gname { font-weight: 700; }
          .gsize { color: #c0392b; font-size: 10.5px; margin-top: 2px; }
          .detail { font-size: 10.5px; color: #555; }
          .print-btn-row { text-align: center; margin-top: 20px; }
          .print-btn { background: #6c3fc5; color: #fff; border: none; padding: 10px 22px; border-radius: 4px; font-size: 13px; font-weight: 700; cursor: pointer; }
          @media print { .print-btn-row { display: none; } }
        </style>
      </head>
      <body>
        <div class="doc-title">주문 목록</div>
        <table>
          <thead>
            <tr>
              <th style="width: 60px;">번호</th>
              <th>주문내용</th>
              <th style="width: 150px;">주문상세</th>
              <th style="width: 50px;">함량</th>
              <th style="width: 60px;">중량</th>
              <th style="width: 60px;">주문수량</th>
              <th style="width: 110px;">주문일자</th>
            </tr>
          </thead>
          <tbody>
            ${rowsHtml || '<tr><td colspan="7" class="c">공장승인된 주문이 없습니다.</td></tr>'}
          </tbody>
        </table>
        <div class="print-btn-row">
          <button class="print-btn" onclick="window.print()">프린트</button>
        </div>
      </body>
    </html>
  `);
  win.document.close();
}

// Small sticker label (per item) meant to be stuck on the product box.
// One label per order item: 물류 / 주문일시 / 제품번호 / 옵션+중량+사이즈 / 메모.
export function printWorkOrderSticker(order: WorkOrderPrintData, items: any[], codeMap: Record<string, string> = {}) {
  const win = window.open('', '_blank');
  if (!win) return;

  const esc = (v: any) => String(v ?? '').replace(/[&<>"]/g, (c) => (
    { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c] as string
  ));
  const codeName = (code: string) => (code && codeMap[code]) || code || '';

  const formatOrderDate = (d?: string) => {
    if (!d) return '-';
    const date = new Date(d);
    if (isNaN(date.getTime())) return d;
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())} ${pad(date.getHours())}:${pad(date.getMinutes())}`;
  };
  const orderDateFormatted = formatOrderDate(order.orderDate);

  const labelsHtml = items.map((item, index) => {
    const opts = [
      codeName(item.purity),
      item.color && item.color !== 'EMPTY' ? codeName(item.color) : '',
      item.requestedWeight ? `${item.requestedWeight}g` : '',
      item.size && item.size !== 'EMPTY' ? item.size : ''
    ].filter(Boolean).join(' / ') || '-';
    const memo = [item.memo, item.requestedMemo || item.inspectionMemo].filter(Boolean).join(' / ');

    const asBadge = item.isAsOrder ? '<span class="as">AS</span>' : '';

    return `
      <div class="sticker" style="${index < items.length - 1 ? 'page-break-after: always;' : ''}">
        <div class="row"><span class="k">주문번호:</span><span class="v">${esc(order.orderNo || '-')}${asBadge}</span></div>
        <div class="row"><span class="k">물류:</span><span class="v">${esc(order.logisticsCompanyName || '-')}</span></div>
        <div class="row"><span class="k">주문일시:</span><span class="v">${esc(orderDateFormatted)}</span></div>
        <div class="row"><span class="k">제품번호:</span><span class="v">${esc(item.productNo || '-')}</span></div>
        <div class="row opts">${esc(opts)}</div>
        <div class="row memo"><span class="k">메모:</span><span class="v">${esc(memo || '-')}</span></div>
      </div>`;
  }).join('');

  win.document.write(`
    <html>
      <head>
        <title>작업지시서 인쇄2 - ${esc(order.orderNo || '')}</title>
        <style>
          @page { size: 60mm 40mm; margin: 0; }
          * { box-sizing: border-box; }
          body {
            margin: 0;
            font-family: 'Malgun Gothic', 'Apple SD Gothic Neo', sans-serif;
            color: #1a1a1a;
          }
          .sticker {
            width: 60mm;
            height: 40mm;
            padding: 2.5mm 3mm;
            display: flex;
            flex-direction: column;
            justify-content: center;
            gap: 1.2mm;
          }
          .row { display: flex; gap: 1.5mm; font-size: 7.5px; line-height: 1.3; }
          .row .k { font-weight: 700; color: #555; flex-shrink: 0; }
          .row .v { color: #1a1a1a; word-break: break-word; }
          .row.opts { font-size: 8px; font-weight: 700; color: #8a6d3b; margin: 0.5mm 0; }
          .row.memo .v { color: #444; }
          .as {
            display: inline-block; background: #f56c6c; color: #fff;
            font-size: 6.5px; font-weight: 700; padding: 0.3mm 1mm; border-radius: 1mm; margin-left: 1mm;
          }
        </style>
      </head>
      <body>
        ${labelsHtml || ''}
        <script>
          window.onload = () => {
            setTimeout(() => { window.print(); window.close(); }, 300);
          };
        <\/script>
      </body>
    </html>
  `);
  win.document.close();
}
