<template>
<div class="errPage-container">
    <el-button :icon="ArrowLeft" class="pan-back-btn" @click="back">
      돌아가기
    </el-button>
    <el-row>
      <el-col :span="12">
        <h1 class="text-jumbo text-ginormous">
          Oops!
        </h1>
        gif 출처 <a href="https://zh.airbnb.com/" target="_blank">airbnb</a> 페이지
        <h2>이 페이지에 접근할 권한이 없습니다</h2>
        <h6>불만이 있으면 팀장에게 문의하세요</h6>
        <ul class="list-unstyled">
          <li>아니면 다음으로 이동할 수 있습니다:</li>
          <li class="link-type">
            <router-link to="/dashboard">
              홈으로 돌아가기
            </router-link>
          </li>
          <li class="link-type">
            <a href="https://www.taobao.com/">둘러보기</a>
          </li>
          <li><a href="#" @click.prevent="dialogVisible = true">클릭해서 이미지 보기</a></li>
        </ul>
      </el-col>
      <el-col :span="12">
        <img :src="errGif" width="313" height="428" alt="Girl has dropped her ice cream.">
      </el-col>
    </el-row>
    <base-popup draggable v-model="dialogVisible" title="둘러보기">
      <img :src="ewizardClap" class="pan-img">
    </base-popup>
  </div>
</template>

<script>

import { ArrowLeft } from '@element-plus/icons-vue';
import { markRaw } from 'vue';
import errGif from '@/assets/401_images/401.gif';
import { defineComponent } from 'vue';
import BasePopup from '@/components/BasePopup/index.vue';

export default defineComponent({
  name: 'Page401',
  components: {
    BasePopup
  },
  data() {
    return {
      ArrowLeft: markRaw(ArrowLeft),
      errGif: errGif + '?' + +new Date(),
      ewizardClap: 'https://wpimg.wallstcn.com/007ef517-bafd-4066-aae4-6883632d9646',
      dialogVisible: false
    };
  },
  methods: {

    back() {
      if (this.$route.query.noGoBack) {
        this.$router.push({ path: '/dashboard' });
      } else {
        this.$router.go(-1);
      }
    }
  }
});
</script>

<style lang="scss" scoped>
.errPage-container {
  width: 800px;
  max-width: 100%;
  margin: 6.25rem auto;

  .pan-back-btn {
    background: #008489;
    color: #fff;
    border: none !important;
  }

  .pan-gif {
    margin: 0 auto;
    display: block;
  }

  .pan-img {
    display: block;
    margin: 0 auto;
    width: 100%;
  }

  .text-jumbo {
    font-size: 3.75rem;
    font-weight: 700;
    color: #484848;
  }

  .list-unstyled {
    font-size: 0.875rem;

    li {
      padding-bottom: 0.3125rem;
    }

    a {
      color: #008489;
      text-decoration: none;

      &:hover {
        text-decoration: underline;
      }
    }
  }
}
</style>

