// Opens a print-ready A4 work order (작업지시서) in a new window and triggers
// the browser print dialog. Kept framework-free so it can be called from any
// component with the order + prepared item rows.

interface WorkOrderPrintData {
  orderNo?: string;
  logisticsCompanyName?: string;
  manufacturerName?: string;
  factoryRemarks?: string;
  workOrderRemarks?: string;
}

export function printWorkOrder(order: WorkOrderPrintData, items: any[], codeMap: Record<string, string> = {}) {
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

  let seq = 0;
  const rowsHtml = items.map((item) => {
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

    return `
      <tr class="${isChild ? 'child' : ''}">
        <td class="c">${isChild ? '&#8627;' : seq}</td>
        <td>
          <div class="pname">${setBadge}${esc(name)} ${asBadge}</div>
          <div class="pno">${esc(item.productNo || '')}</div>
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
              <th>제품 정보</th>
              <th style="width: 48px;">수량</th>
              <th style="width: 90px;">옵션</th>
              <th style="width: 78px;">의뢰중량</th>
              <th style="width: 150px;">메모</th>
            </tr>
          </thead>
          <tbody>
            ${rowsHtml || '<tr><td colspan="6" class="c">항목이 없습니다.</td></tr>'}
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
          window.onload = () => { window.print(); window.close(); };
        <\/script>
      </body>
    </html>
  `);
  win.document.close();
}
