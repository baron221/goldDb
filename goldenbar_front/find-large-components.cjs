/* eslint-disable */
const fs = require('fs');
const path = require('path');

function getVueFiles(dir, fileList = []) {
  const files = fs.readdirSync(dir);
  for (const file of files) {
    const filePath = path.join(dir, file);
    const stat = fs.statSync(filePath);
    if (stat.isDirectory()) {
      if (file !== 'node_modules' && file !== '.git' && file !== 'dist') {
        getVueFiles(filePath, fileList);
      }
    } else if (file.endsWith('.vue')) {
      fileList.push(filePath);
    }
  }
  return fileList;
}

const srcDir = path.join(__dirname, 'src');
const vueFiles = getVueFiles(srcDir);
const largeFiles = [];

for (const file of vueFiles) {
  const content = fs.readFileSync(file, 'utf-8');
  const lines = content.split('\n').length;
  if (lines > 400) {
    largeFiles.push({
      relativePath: path.relative(__dirname, file).replace(/\\/g, '/'),
      lines
    });
  }
}

largeFiles.sort((a, b) => b.lines - a.lines);

console.log(JSON.stringify(largeFiles, null, 2));
console.log(`Total files over 400 lines: ${largeFiles.length}`);
