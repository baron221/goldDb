import { ElMessage } from 'element-plus';

// Prints exactly what's currently rendered in the table (respecting the
// active filter/page), reusing the same DOM-extraction approach as the
// "화면 그대로" Excel export so both features stay in sync.
export function useTablePrint(tableRef: any) {
  const printDomTable = (title?: string) => {
    const tableEl = tableRef.value?.$el;
    if (!tableEl) return;

    const headerCells = tableEl.querySelectorAll('.el-table__header th.el-table__cell');
    const headers: string[] = [];
    const validIndexes: number[] = [];

    Array.from(headerCells).forEach((th: any, index: number) => {
      const text = th.innerText.trim();
      if (text && text !== '' && !th.classList.contains('el-table-column--selection') && text !== '작업' && text !== '액션' && text !== '관리' && text !== '더보기') {
        headers.push(text);
        validIndexes.push(index);
      }
    });

    const rows = tableEl.querySelectorAll('.el-table__body tr.el-table__row');
    const data = Array.from(rows).map((tr: any) => {
      const cells = tr.querySelectorAll('td.el-table__cell');
      return validIndexes.map(idx => {
        const cell = cells[idx];
        return cell ? cell.innerText.trim().replace(/\n/g, ' / ') : '';
      });
    });

    if (data.length === 0) {
      ElMessage.warning('인쇄할 데이터가 없습니다.');
      return;
    }

    const win = window.open('', '_blank');
    if (!win) return;

    const esc = (v: any) => String(v ?? '').replace(/[&<>"]/g, (c) => (
      { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c] as string
    ));

    const headerHtml = headers.map(h => `<th>${esc(h)}</th>`).join('');
    const rowsHtml = data.map((row: string[]) =>
      `<tr>${row.map(cell => `<td>${esc(cell)}</td>`).join('')}</tr>`
    ).join('');

    win.document.write(`
      <html>
        <head>
          <title>${esc(title || '인쇄')}</title>
          <style>
            @page { size: A4 landscape; margin: 10mm; }
            * { box-sizing: border-box; }
            body { margin: 0; font-family: 'Malgun Gothic', 'Apple SD Gothic Neo', sans-serif; color: #1a1a1a; font-size: 11px; }
            .print-title { font-size: 18px; font-weight: 800; margin-bottom: 10px; border-bottom: 2px solid #1a1a1a; padding-bottom: 6px; }
            .print-meta { font-size: 10px; color: #666; margin-bottom: 10px; }
            table { width: 100%; border-collapse: collapse; }
            thead th { background: #f2f0eb; border: 1px solid #b9b5ac; padding: 6px 5px; font-size: 10.5px; font-weight: 700; }
            tbody td { border: 1px solid #cfccc4; padding: 5px 6px; vertical-align: middle; word-break: break-word; }
            tbody tr:nth-child(even) { background: #fbfaf7; }
          </style>
        </head>
        <body>
          <div class="print-title">${esc(title || '')}</div>
          <div class="print-meta">인쇄일시: ${new Date().toLocaleString('ko-KR')} · 총 ${data.length}건</div>
          <table>
            <thead><tr>${headerHtml}</tr></thead>
            <tbody>${rowsHtml}</tbody>
          </table>
          <script>
            window.onload = () => { window.print(); window.close(); };
          <\/script>
        </body>
      </html>
    `);
    win.document.close();
  };

  return { printDomTable };
}
