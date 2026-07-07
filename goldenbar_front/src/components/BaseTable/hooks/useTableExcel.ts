import { ElMessage } from 'element-plus';
import dayjs from 'dayjs';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';

export function useTableExcel(tableRef: any, slots: any, attrs: any) {
  const exportDomExcel = async (filename?: string) => {
    const tableEl = tableRef.value?.$el;
    if (!tableEl) return;

    const headerCells = tableEl.querySelectorAll('.el-table__header th.el-table__cell');
    const headers: string[] = [];
    const validIndexes: number[] = [];

    Array.from(headerCells).forEach((th: any, index: number) => {
      const text = th.innerText.trim();
      if (text && text !== '' && !th.classList.contains('el-table-column--selection') && text !== '작업' && text !== '액션' && text !== '관리') {
        headers.push(text);
        validIndexes.push(index);
      }
    });

    const rows = tableEl.querySelectorAll('.el-table__body tr.el-table__row');
    const data = Array.from(rows).map((tr: any) => {
      const cells = tr.querySelectorAll('td.el-table__cell');
      return validIndexes.map(idx => {
        const cell = cells[idx];
        return cell ? cell.innerText.trim() : '';
      });
    });

    if (data.length === 0) {
      ElMessage.warning('다운로드할 데이터가 없습니다.');
      return;
    }

    try {
      const excel = await import('@/vendor/Export2Excel');
      const fileNm = filename || `ExportDom_${dayjs().format('YYYYMMDD_HHmmss')}`;

      excel.export_json_to_excel({
        header: headers,
        data,
        filename: fileNm,
        autoWidth: true,
        bookType: 'xlsx'
      });
      ElMessage.success('화면 표시 기준 엑셀 파일이 다운로드되었습니다.');
    } catch (err) {
      console.error('엑셀 내보내기 오류:', err);
      ElMessage.error('엑셀 내보내기 중 오류가 발생했습니다.');
    }
  };

  const exportExcel = async (filename?: string) => {
    const defaultSlot = slots.default ? slots.default() : [];
    const headers: string[] = [];
    const fields: string[] = [];
    const formatters: any[] = [];

    const extractColumns = (nodes: any[]) => {
      nodes.forEach(node => {
        if (node.type === Symbol.for('v-fgt') || node.type?.toString() === 'Symbol(v-fgt)' || node.type === 'Fragment') {
          if (Array.isArray(node.children)) {
            extractColumns(node.children);
          }
        } else if (node.props) {
          const label = node.props.label || '';
          const prop = node.props.prop;
          const formatter = node.props['excel-formatter'] || node.props.excelFormatter;
          const isImageLabel = label.includes('사진') || label.includes('이미지') ||
                              label.toLowerCase().includes('photo') || label.toLowerCase().includes('image');

          if (label && (prop || formatter || isImageLabel)) {
            headers.push(label);
            fields.push(prop || (isImageLabel ? '__IMAGE__' : ''));
            formatters.push(formatter || null);
          }
        }
      });
    };

    extractColumns(defaultSlot);

    const getNestedValue = (obj: any, path: string) => {
      if (!path || path === '__IMAGE__') return undefined;
      return path.split('.').reduce((acc, part) => acc && acc[part], obj);
    };

    const tableData = (attrs.data || tableRef.value?.$attrs.data || tableRef.value?.data || []) as any[];

    if (tableData.length === 0) {
      ElMessage.warning('다운로드할 데이터가 없습니다.');
      return;
    }

    try {
      const workbook = new ExcelJS.Workbook();
      const worksheet = workbook.addWorksheet('Sheet1');

      const headerRow = worksheet.addRow(headers);
      headerRow.eachCell((cell) => {
        cell.fill = {
          type: 'pattern',
          pattern: 'solid',
          fgColor: { argb: 'FFEADECC' }
        };
        cell.font = { bold: true };
        cell.alignment = { vertical: 'middle', horizontal: 'center' };
        cell.border = {
          top: { style: 'thin' },
          left: { style: 'thin' },
          bottom: { style: 'thin' },
          right: { style: 'thin' }
        };
      });

      for (let i = 0; i < tableData.length; i++) {
        const rowData = tableData[i];
        const rowValues = fields.map((field, idx) => {
          const formatter = formatters[idx];
          if (typeof formatter === 'function') {
            return formatter(rowData);
          }

          const val = getNestedValue(rowData, field);
          if (val instanceof Date) {
            return dayjs(val).format('YYYY-MM-DD HH:mm:ss');
          }
          return val !== undefined && val !== null ? val : '';
        });

        const currentRow = worksheet.addRow(rowValues);
        currentRow.height = 30;

        for (let j = 0; j < fields.length; j++) {
          const field = fields[j];
          const label = headers[j];
          let imageUrl = '';

          const isImageColumn =
            field === '__IMAGE__' ||
            label.includes('사진') ||
            label.includes('이미지') ||
            label.toLowerCase().includes('photo') ||
            label.toLowerCase().includes('image') ||
            field.toLowerCase().includes('photo') ||
            field.toLowerCase().includes('image') ||
            field.toLowerCase().includes('url') ||
            (rowData[field] && typeof rowData[field] === 'string' && (rowData[field].startsWith('http') || rowData[field].includes('/api/file/download') || rowData[field].includes('/api/Attachment/download')));

          if (isImageColumn) {
            imageUrl = rowData[field];
            if (!imageUrl && (label.includes('사진') || label.includes('이미지') || field === '__IMAGE__')) {
              imageUrl = rowData.photoUrl ||
                         rowData.productPhotoUrl ||
                         (rowData.photos && rowData.photos.length > 0 ? (rowData.photos.find((p: any) => p.isMain)?.photoUrl || rowData.photos[0].photoUrl) : '') ||
                         (rowData.attachments && rowData.attachments.length > 0 ?
                           (rowData.attachments.find((a: any) => a.isMain)?.filePath || rowData.attachments[0].filePath) : '');
            }

            if (imageUrl) {
              try {
                const response = await fetch(imageUrl);
                if (!response.ok) throw new Error('Network response was not ok');
                const buffer = await response.arrayBuffer();
                const extension = imageUrl.split('.').pop()?.split('?')[0].toLowerCase() || 'png';

                const imageId = workbook.addImage({
                  buffer: buffer,
                  extension: (['jpeg', 'jpg', 'png', 'gif'].includes(extension) ? extension : 'png') as any
                });

                worksheet.addImage(imageId, {
                  tl: { col: j, row: i + 1 },
                  br: { col: j + 1, row: i + 2 },
                  editAs: 'oneCell'
                });

                currentRow.height = 75;
                currentRow.getCell(j + 1).value = '';
              } catch (imgErr) {
                console.error('이미지 로드 실패:', imageUrl, imgErr);
              }
            }
          }

          const cell = currentRow.getCell(j + 1);
          cell.alignment = {
            vertical: 'middle',
            horizontal: 'center',
            wrapText: true
          };
          cell.border = {
            top: { style: 'thin' },
            left: { style: 'thin' },
            bottom: { style: 'thin' },
            right: { style: 'thin' }
          };
        }
      }

      worksheet.columns.forEach((col, i) => {
        const label = headers[i];
        const isImageCol = label.includes('사진') || label.includes('이미지') ||
                           label.toLowerCase().includes('photo') || label.toLowerCase().includes('image');

        if (isImageCol) {
          col.width = 15;
        } else {
          let maxLen = headers[i].length + 5;
          col.eachCell({ includeEmpty: true }, (cell) => {
            const len = cell.value ? cell.value.toString().length : 0;
            if (len > maxLen) maxLen = len;
          });
          col.width = maxLen < 12 ? 12 : (maxLen > 50 ? 50 : maxLen);
        }
      });

      const fileNm = filename || `Export_${dayjs().format('YYYYMMDD_HHmmss')}`;
      const buffer = await workbook.xlsx.writeBuffer();
      saveAs(new Blob([buffer]), `${fileNm}.xlsx`);

      ElMessage.success('이미지를 포함한 엑셀 파일이 다운로드되었습니다.');
    } catch (err) {
      console.error('엑셀 내보내기 오류:', err);
      ElMessage.error('엑셀 내보내기 중 오류가 발생했습니다.');
    }
  };

  return {
    exportDomExcel,
    exportExcel
  };
}
