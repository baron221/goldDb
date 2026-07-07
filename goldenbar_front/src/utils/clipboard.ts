import Clipboard from 'clipboard';

function clipboardSuccess() {
  ElMessage({
    message: '성공적으로 복사되었습니다.',
    type: 'success',
    duration: 1500
  });
}

function clipboardError() {
  ElMessage({
    message: '복사에 실패했습니다.',
    type: 'error'
  });
}

export default function handleClipboard(text: string, event: Event) {
  const clipboard = new Clipboard(event.target, {
    text: () => text
  });
  clipboard.on('success', () => {
    clipboardSuccess();
    clipboard.destroy();
  });
  clipboard.on('error', () => {
    clipboardError();
    clipboard.destroy();
  });
  clipboard.onClick(event);
}
