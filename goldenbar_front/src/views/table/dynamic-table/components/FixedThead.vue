<template>
<div class="app-container">
    <div class="filter-container">
      <el-checkbox-group v-model="checkboxVal">
        <el-checkbox value="apple">
          apple
        </el-checkbox>
        <el-checkbox value="banana">
          banana
        </el-checkbox>
        <el-checkbox value="orange">
          orange
        </el-checkbox>
      </el-checkbox-group>
    </div>

    <base-table :key="key" :data="tableData" style="width: 100%">
      <el-table-column prop="name" label="fruitName" width="180" />
      <el-table-column v-for="fruit in formThead" :key="fruit" :label="fruit" :prop="fruit">
        <template v-slot="scope">
          {{ scope.row[fruit] }}
        </template>
      </el-table-column>
    </base-table>
  </div>
</template>

<script>
import { defineComponent } from 'vue';
import BaseTable from '@/components/BaseTable/index.vue';

const defaultFormThead = ['apple', 'banana'];

export default defineComponent({
  components: { BaseTable },
  data() {
    return {
      tableData: [
        {
          name: 'fruit-1',
          apple: 'apple-10',
          banana: 'banana-10',
          orange: 'orange-10'
        },
        {
          name: 'fruit-2',
          apple: 'apple-20',
          banana: 'banana-20',
          orange: 'orange-20'
        }
      ],
      key: 1, 
      formTheadOptions: ['apple', 'banana', 'orange'],
      checkboxVal: defaultFormThead, 
      formThead: defaultFormThead 
    };
  },
  watch: {
    checkboxVal(valArr) {
      this.formThead = this.formTheadOptions.filter(i => valArr.indexOf(i) >= 0);
      this.key = this.key + 1;
    }
  }
});
</script>

