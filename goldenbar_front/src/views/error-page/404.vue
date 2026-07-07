<template>
<div class="wscn-http404-container">
    <div class="wscn-http404">
      <div class="pic-404">
        <img class="pic-404__parent" src="@/assets/404_images/404.png" alt="404">
        <img class="pic-404__child left" src="@/assets/404_images/404_cloud.png" alt="404">
        <img class="pic-404__child mid" src="@/assets/404_images/404_cloud.png" alt="404">
        <img class="pic-404__child right" src="@/assets/404_images/404_cloud.png" alt="404">
      </div>
      <div class="bullshit">
        <div class="bullshit__oops">OOPS!</div>
        <div class="bullshit__info">All rights reserved
          <a style="color:#20a0ff" href="https://jsini.co.kr" target="_blank">quristyle</a>
        </div>
        <div class="bullshit__headline">{{ message }}</div>
        <div class="bullshit__info">입력하신 URL이 정확한지 확인하시거나, 아래 버튼을 클릭하여 홈으로 이동하거나 다시 로그인해 주세요.</div>
        <div class="bullshit__btn-container">
          <router-link to="/" class="bullshit__return-home">홈으로 이동</router-link>
          <a class="bullshit__relogin" @click.prevent="handleRelogin">재로그인</a>
        </div>
      </div>
    </div>
  </div>
</template>

<script>

import { defineComponent } from 'vue';
import store from '@/store';

export default defineComponent({
  name: 'Page404',
  computed: {

    message() {
      return '페이지를 찾을 수 없습니다...';
    }
  },
  methods: {

    async handleRelogin() {
      try {
        await store.user().logout();
        this.$router.push('/login');
      } catch (err) {
        console.error('Logout failed:', err);
        this.$router.push('/login');
      }
    }
  }
});
</script>

<style lang="scss" scoped>
.wscn-http404-container {
  transform: translate(-50%, -50%);
  position: absolute;
  top: 40%;
  left: 50%;
}

.wscn-http404 {
  position: relative;
  width: 1200px;
  padding: 0 3.125rem;
  overflow: hidden;

  .pic-404 {
    position: relative;
    float: left;
    width: 600px;
    overflow: hidden;

    &__parent {
      width: 100%;
    }

    &__child {
      position: absolute;

      &.left {
        width: 80px;
        top: 17px;
        left: 220px;
        opacity: 0;
        animation-name: cloudLeft;
        animation-duration: 2s;
        animation-timing-function: linear;
        animation-fill-mode: forwards;
        animation-delay: 1s;
      }

      &.mid {
        width: 46px;
        top: 10px;
        left: 420px;
        opacity: 0;
        animation-name: cloudMid;
        animation-duration: 2s;
        animation-timing-function: linear;
        animation-fill-mode: forwards;
        animation-delay: 1.2s;
      }

      &.right {
        width: 62px;
        top: 100px;
        left: 500px;
        opacity: 0;
        animation-name: cloudRight;
        animation-duration: 2s;
        animation-timing-function: linear;
        animation-fill-mode: forwards;
        animation-delay: 1s;
      }

      @keyframes cloudLeft {
        0% {
          top: 17px;
          left: 220px;
          opacity: 0;
        }

        20% {
          top: 33px;
          left: 188px;
          opacity: 1;
        }

        80% {
          top: 81px;
          left: 92px;
          opacity: 1;
        }

        100% {
          top: 97px;
          left: 60px;
          opacity: 0;
        }
      }

      @keyframes cloudMid {
        0% {
          top: 10px;
          left: 420px;
          opacity: 0;
        }

        20% {
          top: 40px;
          left: 360px;
          opacity: 1;
        }

        70% {
          top: 130px;
          left: 180px;
          opacity: 1;
        }

        100% {
          top: 160px;
          left: 120px;
          opacity: 0;
        }
      }

      @keyframes cloudRight {
        0% {
          top: 100px;
          left: 500px;
          opacity: 0;
        }

        20% {
          top: 120px;
          left: 460px;
          opacity: 1;
        }

        80% {
          top: 180px;
          left: 340px;
          opacity: 1;
        }

        100% {
          top: 200px;
          left: 300px;
          opacity: 0;
        }
      }
    }
  }

  .bullshit {
    position: relative;
    float: left;
    width: 300px;
    padding: 1.875rem 0;
    overflow: hidden;

    &__oops {
      font-size: 2rem;
      font-weight: bold;
      line-height: 40px;
      color: #1482f0;
      opacity: 0;
      margin-bottom: 1.25rem;
      animation-name: slideUp;
      animation-duration: 0.5s;
      animation-fill-mode: forwards;
    }

    &__headline {
      font-size: 1.25rem;
      line-height: 24px;
      color: #222;
      font-weight: bold;
      opacity: 0;
      margin-bottom: 0.625rem;
      animation-name: slideUp;
      animation-duration: 0.5s;
      animation-delay: 0.1s;
      animation-fill-mode: forwards;
    }

    &__info {
      font-size: 0.9rem;
      line-height: 21px;
      color: grey;
      opacity: 0;
      margin-bottom: 1.875rem;
      animation-name: slideUp;
      animation-duration: 0.5s;
      animation-delay: 0.2s;
      animation-fill-mode: forwards;
    }

    &__btn-container {
      display: flex;
      gap: 0.625rem;
      opacity: 0;
      animation-name: slideUp;
      animation-duration: 0.5s;
      animation-delay: 0.3s;
      animation-fill-mode: forwards;
    }

    &__return-home {
      display: block;
      width: 110px;
      height: 36px;
      background: #1482f0;
      border-radius: 100px;
      text-align: center;
      color: #ffffff;
      font-size: 0.875rem;
      line-height: 36px;
      cursor: pointer;
      text-decoration: none;
    }

    &__relogin {
      display: block;
      width: 110px;
      height: 36px;
      background: #f56c6c;
      border-radius: 100px;
      text-align: center;
      color: #ffffff;
      font-size: 0.875rem;
      line-height: 36px;
      cursor: pointer;
      text-decoration: none;
    }

    @keyframes slideUp {
      0% {
        transform: translateY(60px);
        opacity: 0;
      }

      100% {
        transform: translateY(0);
        opacity: 1;
      }
    }
  }
}
</style>

