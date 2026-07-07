<template>
<div class="social-signup-container">

    카카오, 네이버, 구글 등을 준비 하라.

    <div class="sign-btn" @click="wechatHandleClick('wechat')">
      <span class="wx-svg-container">
        <svg-icon icon-class="wechat" class="icon" />
      </span>
      WeChat
    </div>
    <div class="sign-btn" @click="tencentHandleClick('tencent')">
      <span class="qq-svg-container">
        <svg-icon icon-class="qq" class="icon" />
      </span>
      QQ
    </div>
  </div>
</template>

<script lang="ts">

import { defineComponent } from 'vue';

export default defineComponent({
  name: 'SocialSignin',
  methods: {
    wechatHandleClick(type: string) { 
      alert(type + 'ok ');

    },
    tencentHandleClick(type: string) { 
      alert(type + 'ok');

    }
  }
});
</script>

<style lang="scss" scoped>
.social-signup-container {
  margin: 1.25rem 0;

  .sign-btn {
    display: inline-block;
    cursor: pointer;
  }

  .icon {
    color: #fff;
    font-size: 1.5rem;
    margin-top: 0.5rem;
  }

  .wx-svg-container,
  .qq-svg-container {
    display: inline-block;
    width: 40px;
    height: 40px;
    line-height: 40px;
    text-align: center;
    padding-top: 0.0625rem;
    border-radius: 2px;
    margin-bottom: 1.25rem;
    margin-right: 0.3125rem;
  }

  .wx-svg-container {
    background-color: #24da70;
  }

  .qq-svg-container {
    background-color: #6BA2D6;
    margin-left: 3.125rem;
  }
}
</style>

