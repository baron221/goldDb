const modules = import.meta.glob('../../icons/svg/*.svg', { eager: true });

const nameArr = [];
Object.keys(modules).forEach((key) => {
  const name = key.split('/').pop().split('.').shift();
  nameArr.push(name);

});

export default nameArr;
