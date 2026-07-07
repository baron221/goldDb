<template>
<div class="app-container">
    <div style="margin: 0 0 0.3125rem 1.25rem">
      Fixed header, sorted by header order,
    </div>
    <fixed-thead />

    <div style="margin: 1.875rem 0 0.3125rem 1.25rem">
      Not fixed header, sorted by click order
    </div>
    <unfixed-thead />
  </div>
</template>

<script>
import { defineComponent } from 'vue';
import FixedThead from './components/FixedThead';
import UnfixedThead from './components/UnfixedThead';

export default defineComponent({
  name: 'DynamicTable',
  components: { FixedThead, UnfixedThead }
});
</script>

