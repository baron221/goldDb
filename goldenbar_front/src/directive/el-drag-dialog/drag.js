import emitter from 'tiny-emitter/instance';

function loopFindParentDialog(el) {
  if (!el) return null;
  if (el.classList.contains('el-dialog')) {
    return el;
  }
  return loopFindParentDialog(el.parentElement);
}

export default {

  mounted(el, binding, vnode) {
    const dialogEl = loopFindParentDialog(el);
    const dialogHeaderEl = dialogEl.querySelector('.el-dialog__header');

    const dragDom = dialogEl;
    dialogHeaderEl.style.cssText += ';cursor:move;';
    dragDom.style.cssText += ';top:0px;';

    const getStyle = (function() {
      if (window.document.currentStyle) {
        return (dom, attr) => dom.currentStyle[attr];
      } else {
        return (dom, attr) => getComputedStyle(dom, false)[attr];
      }
    })();

    dialogHeaderEl.onmousedown = (e) => {

      const disX = e.clientX - dialogHeaderEl.offsetLeft;
      const disY = e.clientY - dialogHeaderEl.offsetTop;

      const dragDomWidth = dragDom.offsetWidth;
      const dragDomHeight = dragDom.offsetHeight;

      const screenWidth = document.body.clientWidth;
      const screenHeight = document.body.clientHeight;

      const minDragDomLeft = dragDom.offsetLeft;
      const maxDragDomLeft = screenWidth - dragDom.offsetLeft - dragDomWidth;

      const minDragDomTop = dragDom.offsetTop;
      const maxDragDomTop = screenHeight - dragDom.offsetTop - dragDomHeight;

      let styL = getStyle(dragDom, 'left');
      let styT = getStyle(dragDom, 'top');

      if (styL.includes('%')) {
        styL = +document.body.clientWidth * (+styL.replace(/\%/g, '') / 100);
        styT = +document.body.clientHeight * (+styT.replace(/\%/g, '') / 100);
      } else {
        styL = +styL.replace(/\px/g, '');
        styT = +styT.replace(/\px/g, '');
      }

      document.onmousemove = function(e) {

        let left = e.clientX - disX;
        let top = e.clientY - disY;

        if (-(left) > minDragDomLeft) {
          left = -minDragDomLeft;
        } else if (left > maxDragDomLeft) {
          left = maxDragDomLeft;
        }

        if (-(top) > minDragDomTop) {
          top = -minDragDomTop;
        } else if (top > maxDragDomTop) {
          top = maxDragDomTop;
        }

        dragDom.style.cssText += `;left:${left + styL}px;top:${top + styT}px;`;

        emitter.emit('elDialogDragDialog');
      };

      document.onmouseup = function() {
        document.onmousemove = null;
        document.onmouseup = null;
      };
    };
  }
};
