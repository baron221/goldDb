<template>
<div class="dashboard-container">
    <component :is="currentRole" />
  </div>
</template>

<script>
import { defineComponent } from 'vue';
import { mapState } from 'pinia';
import adminDashboard from './admin';
import editorDashboard from './editor';
import logisticsDashboard from './logistics';
import factoryDashboard from './factory';
import othersDashboard from './others';
import store from '@/store';

export default defineComponent({
  name: 'Dashboard',
  components: {
    adminDashboard,
    editorDashboard,
    logisticsDashboard,
    factoryDashboard,
    othersDashboard
  },
  data() {
    return {
      currentRole: 'othersDashboard'
    };
  },
  computed: {
    ...mapState(store.user, ['roles', 'companyType'])
  },
  created() {

    if (this.roles.includes('admin')) {
      this.currentRole = 'adminDashboard';
    } else if (this.companyType === 'DCC') {
      this.currentRole = 'logisticsDashboard';
    } else if (this.companyType === 'MFG') {
      this.currentRole = 'factoryDashboard';
    } else if (this.companyType === 'RTL') {
      this.currentRole = 'editorDashboard';
    } else {

      if (this.roles.includes('logistics')) {
        this.currentRole = 'logisticsDashboard';
      } else if (this.roles.includes('factory')) {
        this.currentRole = 'factoryDashboard';
      } else if (this.roles.includes('store') || this.roles.includes('editor')) {
        this.currentRole = 'editorDashboard';
      } else {
        this.currentRole = 'othersDashboard';
      }
    }
  }
});
</script>

