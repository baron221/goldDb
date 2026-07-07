import { defineStore } from 'pinia';
import { getCodes } from '@/api/code';

interface CodeState {

  codes: any[];

  codeMap: Record<string, string>;

  loading: boolean;
}

const useCodeStore = defineStore('code', {
  state: (): CodeState => ({
    codes: [],
    codeMap: {},
    loading: false
  }),
  actions: {

    async fetchCodes() {
      if (this.codes.length > 0) return;

      this.loading = true;
      try {
        const response = await getCodes();
        if (response.data) {
          this.codes = response.data;
          this.codeMap = this.flattenCodes(response.data);
        }
      } catch (error) {
        console.error('Failed to fetch codes:', error);
      } finally {
        this.loading = false;
      }
    },

    flattenCodes(list: any[]): Record<string, string> {
      const map: Record<string, string> = {};
      const flatten = (items: any[]) => {
        items.forEach(item => {
          map[item.code] = item.name;
          if (item.children && item.children.length > 0) {
            flatten(item.children);
          }
        });
      };
      flatten(list);
      return map;
    },

    getCodeName(code: string): string {
      return this.codeMap[code] || code;
    },

    getName(code: string): string {
      return this.getCodeName(code);
    },

    getCodesByGroupStore(groupCode: string): any[] {
      const group = this.codes.find(c => c.code === groupCode);
      return group ? group.children || [] : [];
    },

    getCodesByParentIdStore(parentId: number | string): any[] {
      let found: any = null;
      const findById = (items: any[]) => {
        for (const item of items) {
          if (item.id === Number(parentId)) {
            found = item;
            return;
          }
          if (item.children && item.children.length > 0) {
            findById(item.children);
            if (found) return;
          }
        }
      };
      findById(this.codes);
      return found ? found.children || [] : [];
    }
  }
});

export default useCodeStore;
