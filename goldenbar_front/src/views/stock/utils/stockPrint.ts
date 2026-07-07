export function printBulkBarcode(selectedGroups: any[], codeMap: any) {
  const allItems = selectedGroups.flatMap(g => g.items);
  if (allItems.length === 0) return;

  const win = window.open('', '_blank');
  if (!win) return;

  let labelsHtml = '';

  allItems.forEach((item, index) => {
    const stockNo = item.stockNo || '';
    const barcodeUrl = `https://bwipjs-api.metafloor.com/?bcid=qrcode&text=${encodeURIComponent(stockNo)}&scale=3&includetext=false`;

    const productName = item.productName || '-';
    const productNo = item.productNo || '-';
    const purity = codeMap[item.purity] || item.purity || '-';
    const color = codeMap[item.color] || item.color || '-';
    const weight = item.confirmedWeight || item.actualWeight || 0;
    const manufacturer = item.companyName || '-';

    labelsHtml += `
      <div class="label-container" style="${index < allItems.length - 1 ? 'page-break-after: always;' : ''}">
        <div class="tag-section left">
          <img class="barcode-img" src="${barcodeUrl}" />
          <div class="stock-no">${stockNo}</div>
          <div style="font-size: 0.375rem; text-align: center; color: #888;">GOLD B SYSTEM</div>
        </div>
        <div class="tag-bridge"></div>
        <div class="tag-section right">
          <div class="info-name">${productName}</div>
          <div class="info-detail">
            <div><b>No:</b> ${productNo}</div>
            <div><b>Spec:</b> ${purity} / ${color}</div>
            <div><b>Weight:</b> ${weight.toFixed(2)}g</div>
            <div><b>Mfg:</b> ${manufacturer}</div>
          </div>
        </div>
      </div>
    `;
  });

  win.document.write(`
    <html>
      <head>
        <title>Bulk QR Label Print</title>
        <style>
          @page { size: 80mm 25mm; margin: 0; }
          body { margin: 0; padding: 0; font-family: 'Malgun Gothic', 'Apple SD Gothic Neo', sans-serif; }
          .label-container {
            width: 80mm;
            height: 25mm;
            display: flex;
            align-items: center;
            box-sizing: border-box;
            background: #fff;
            padding: 0 1mm;
          }
          .tag-section {
            flex: 0 0 38mm;
            height: 23mm;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            box-sizing: border-box;
            overflow: hidden;
            border: 1px solid #f0f0f0;
            border-radius: 2mm;
            padding: 1mm;
          }
          .tag-section.left { align-items: center; }
          .tag-section.right { align-items: flex-start; }
          .tag-bridge {
            flex: 1;
            height: 3mm;
            background-color: #f9f9f9;
            border-top: 1px dashed #ddd;
            border-bottom: 1px dashed #ddd;
          }
          .barcode-img {
            width: 14mm;
            height: 14mm;
            object-fit: contain;
            margin-bottom: 1mm;
          }
          .stock-no {
            font-size: 0.5rem;
            text-align: center;
            font-weight: bold;
            letter-spacing: 0.5px;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: clip;
            width: 100%;
          }
          .info-name {
            font-size: 0.825rem;
            font-weight: bold;
            margin-bottom: 1mm;
            white-space: nowrap;
            text-overflow: ellipsis;
            overflow: hidden;
            border-bottom: 1px solid #eee;
            padding-bottom: 0.5mm;
            width: 100%;
          }
          .info-detail {
            font-size: 0.4688rem;
            line-height: 1.4;
            color: #333;
            width: 100%;
          }
          .info-detail b { color: #000; }
        </style>
      </head>
      <body>
        ${labelsHtml}
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

export function printBarcode(qrInfo: any, codeMap: any, mfgLabel: string) {
  const stockNo = qrInfo?.stockNo || '';
  const barcodeUrl = `https://bwipjs-api.metafloor.com/?bcid=qrcode&text=${encodeURIComponent(stockNo)}&scale=3&includetext=false`;

  const productName = qrInfo?.productName || '-';
  const productNo = qrInfo?.productNo || '-';
  const purity = codeMap[qrInfo?.purity] || qrInfo?.purity || '-';
  const color = codeMap[qrInfo?.color] || qrInfo?.color || '-';
  const weight = qrInfo?.confirmedWeight || qrInfo?.actualWeight || 0;
  const manufacturer = qrInfo?.companyName || '-';

  const win = window.open('', '_blank');
  if (!win) return;

  win.document.write(`
    <html>
      <head>
        <title>QR Label - ${stockNo}</title>
        <style>
          @page { size: 80mm 25mm; margin: 0; }
          body { margin: 0; padding: 0; font-family: 'Malgun Gothic', 'Apple SD Gothic Neo', sans-serif; }
          .label-container {
            width: 80mm;
            height: 25mm;
            display: flex;
            align-items: center;
            box-sizing: border-box;
            background: #fff;
            padding: 0 1mm;
          }
          .tag-section {
            flex: 0 0 38mm;
            height: 23mm;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            box-sizing: border-box;
            overflow: hidden;
            border: 1px solid #f0f0f0;
            border-radius: 2mm;
            padding: 1mm;
          }
          .tag-section.left { align-items: center; }
          .tag-section.right { align-items: flex-start; }
          .tag-bridge {
            flex: 1;
            height: 3mm;
            background-color: #f9f9f9;
            border-top: 1px dashed #ddd;
            border-bottom: 1px dashed #ddd;
          }
          .barcode-img {
            width: 14mm;
            height: 14mm;
            object-fit: contain;
            margin-bottom: 1mm;
          }
          .stock-no {
            font-size: 0.5rem;
            text-align: center;
            font-weight: bold;
            letter-spacing: 0.5px;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: clip;
            width: 100%;
          }
          .info-name {
            font-size: 0.825rem;
            font-weight: bold;
            margin-bottom: 1mm;
            white-space: nowrap;
            text-overflow: ellipsis;
            overflow: hidden;
            border-bottom: 1px solid #eee;
            padding-bottom: 0.5mm;
            width: 100%;
          }
          .info-detail {
            font-size: 0.4688rem;
            line-height: 1.4;
            color: #333;
            width: 100%;
          }
          .info-detail b { color: #000; }
        </style>
      </head>
      <body>
        <div class="label-container">
          <div class="tag-section left">
            <img class="barcode-img" id="barcodeImage" src="${barcodeUrl}" />
            <div class="stock-no">${stockNo}</div>
            <div style="font-size: 0.375rem; text-align: center; color: #888;">GOLD B SYSTEM</div>
          </div>
          <div class="tag-bridge"></div>
          <div class="tag-section right">
            <div class="info-name">${productName}</div>
            <div class="info-detail">
              <div><b>No:</b> ${productNo}</div>
              <div><b>Spec:</b> ${purity} / ${color}</div>
              <div><b>Weight:</b> ${weight.toFixed(2)}g</div>
              <div><b>${mfgLabel}:</b> ${manufacturer}</div>
            </div>
          </div>
        </div>
        <script>
          const img = document.getElementById('barcodeImage');
          const triggerPrint = () => {
            window.print();
            window.close();
          };
          if (img.complete) {
            triggerPrint();
          } else {
            img.onload = triggerPrint;
            img.onerror = triggerPrint;
          }
        <\/script>
      </body>
    </html>
  `);
  win.document.close();
}

export function printQr3(
  qrInfo: any,
  codeMap: any,
  dataUrl: string,
  labels: {
    productName: string,
    spec: string,
    weight: string,
    date: string,
    mfg: string
  },
  productionDateFormatted: string
) {
  const win = window.open('', '_blank');
  if (!win) return;

  const productName = qrInfo?.productName || '-';
  const purity = codeMap[qrInfo?.purity] || qrInfo?.purity || '-';
  const weight = qrInfo?.confirmedWeight || qrInfo?.actualWeight || 0;
  const manufacturer = qrInfo?.companyName || '-';
  const stockNo = qrInfo?.stockNo || '';

  win.document.write(`
    <html>
      <head>
        <title>Print 3 - ${stockNo}</title>
        <style>
          @page { size: 50mm 60mm; margin: 0; }
          body { 
            margin: 0; 
            padding: 3mm; 
            box-sizing: border-box; 
            width: 50mm; 
            height: 60mm; 
            font-family: 'Malgun Gothic', 'Apple SD Gothic Neo', sans-serif; 
            display: flex; 
            flex-direction: column; 
            align-items: center; 
            overflow: hidden; 
          }
          .qr-img { width: 28mm; height: 28mm; margin-bottom: 1mm; }
          .stock-no { font-size: 0.825rem; font-weight: bold; margin-bottom: 1.5mm; text-align: center; }
          .info-table { width: 34mm; margin: 0 auto; border-top: 1px solid #000; padding-top: 1.5mm; display: flex; flex-direction: column; gap: 0.5mm; }
          .info-row { display: flex; justify-content: space-between; font-size: 0.5rem; line-height: 1.2; }
          .info-label { font-weight: bold; color: #666; flex-shrink: 0; }
          .info-value { text-align: right; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 20mm; color: #000; }
        </style>
      </head>
      <body>
        <img class="qr-img" src="${dataUrl}" />
        <div class="stock-no">${stockNo}</div>
        <div class="info-table">
          <div class="info-row"><span class="info-label">${labels.productName}:</span> <span class="info-value">${productName}</span></div>
          <div class="info-row"><span class="info-label">${labels.spec}:</span> <span class="info-value">${purity}</span></div>
          <div class="info-row"><span class="info-label">${labels.weight}:</span> <span class="info-value">${weight.toFixed(2)}g</span></div>
          <div class="info-row"><span class="info-label">${labels.date}:</span> <span class="info-value">${productionDateFormatted}</span></div>
          <div class="info-row"><span class="info-label">${labels.mfg}:</span> <span class="info-value">${manufacturer}</span></div>
        </div>
        <script>
          window.onload = () => {
            window.print();
            window.close();
          };
        <\/script>
      </body>
    </html>
  `);
  win.document.close();
}

export function printQr(
  qrInfo: any,
  codeMap: any,
  dataUrl: string,
  labels: {
    orderNo: string,
    productName: string,
    category: string,
    date1: string,
    date2: string,
    mfg: string,
    logistics: string
  },
  dates: {
    productionDate: string,
    orderDate: string
  }
) {
  const win = window.open('', '_blank');
  if (!win) return;

  const manufacturer = qrInfo?.companyName || '-';
  const logistics = qrInfo?.logisticsCompanyName || '-';
  const productNo = qrInfo?.productNo || '-';
  const productName = qrInfo?.productName || '-';

  const categoryLarge = codeMap[qrInfo?.categoryLarge] || qrInfo?.categoryLarge || '';
  const categoryMedium = codeMap[qrInfo?.categoryMedium] || qrInfo?.categoryMedium || '';
  const categorySmall = codeMap[qrInfo?.categorySmall] || qrInfo?.categorySmall || '';
  let category = categoryLarge;
  if (categoryMedium) category += ` > ${categoryMedium}`;
  if (categorySmall) category += ` > ${categorySmall}`;
  if (!category) category = '-';

  win.document.write(`
    <html><head><title>QR - ${qrInfo.stockNo}</title></head>
    <body style="margin:0;display:flex;flex-direction:column;align-items:center;padding: 1.875rem;font-family:sans-serif;">
      <img src="${dataUrl}" style="width:240px;height:240px;" />
      <p style="margin-top: 0.95rem;font-size: 0.9375rem;font-weight:bold;letter-spacing:1px;">${qrInfo.stockNo}</p>
      <div style="margin-top: 0.9375rem; width:240px; font-size: 0.95rem; line-height:1.6; border-top:1px solid #eee; padding-top: 0.625rem;">
        <div style="display:flex; justify-content:space-between;"><span>${labels.orderNo}:</span> <span>${productNo}</span></div>
        <div style="display:flex; justify-content:space-between;"><span>${labels.productName}:</span> <span style="text-align:right;">${productName}</span></div>
        <div style="display:flex; justify-content:space-between;"><span>${labels.category}:</span> <span style="text-align:right;">${category}</span></div>
        <div style="display:flex; justify-content:space-between;"><span>${labels.date1}:</span> <span>${dates.productionDate}</span></div>
        <div style="display:flex; justify-content:space-between;"><span>${labels.date2}:</span> <span>${dates.orderDate}</span></div>
        <div style="display:flex; justify-content:space-between;"><span>${labels.mfg}:</span> <span>${manufacturer}</span></div>
        <div style="display:flex; justify-content:space-between;"><span>${labels.logistics}:</span> <span>${logistics}</span></div>
      </div>
      <script>window.onload=()=>{ window.print(); window.close(); }<\/script>
    </body></html>
  `);
  win.document.close();
}
