import { defineStore } from 'pinia';
import { getCart } from '@/api/cart';

interface ICartState {

  items: any[];
}

export default defineStore({
  id: 'cart',
  state: (): ICartState => ({
    items: []
  }),
  getters: {

    cartCount: (state) => state.items.length
  },
  actions: {

    async fetchCart() {
      try {
        const res = await getCart();
        this.items = res.data;
      } catch (error) {
        console.error('Failed to fetch cart:', error);
      }
    },

    setItems(items: any[]) {
      this.items = items;
    }
  }
});
