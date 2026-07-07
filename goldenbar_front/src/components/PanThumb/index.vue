<template>
<div :style="{ zIndex: zIndex, height: height, width: width }" class="pan-item">
    <div class="pan-info">
      <div class="pan-info-roles-container">
        <slot />
      </div>
    </div>

    <div :style="{ backgroundImage: `url(${image})` }" class="pan-thumb"></div>
  </div>
</template>

<script>
import { defineComponent } from 'vue';

export default defineComponent({
  name: 'PanThumb',
  props: {
    image: {
      type: String,
      required: true
    },
    zIndex: {
      type: Number,
      default: 1
    },
    width: {
      type: String,
      default: '150px'
    },
    height: {
      type: String,
      default: '150px'
    }
  }
});
</script>

<style scoped>
.pan-item {
  width: 200px;
  height: 200px;
  border-radius: 50%;
  display: inline-block;
  position: relative;
  cursor: default;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.2);
}

.pan-info-roles-container {
  padding: 1.25rem;
  text-align: center;
}

.pan-thumb {
  width: 100%;
  height: 100%;
  background-position: center center;
  background-size: cover;
  border-radius: 50%;
  overflow: hidden;
  position: absolute;
  transform-origin: 95% 40%;
  transition: all 0.3s ease-in-out;
}

.pan-info {
  position: absolute;
  width: inherit;
  height: inherit;
  border-radius: 50%;
  overflow: hidden;
  box-shadow: inset 0 0 0 5px rgba(0, 0, 0, 0.05);
}

.pan-info h3 {
  color: #fff;
  text-transform: uppercase;
  position: relative;
  letter-spacing: 2px;
  font-size: 1.125rem;
  margin: 0 3.75rem;
  padding: 1.375rem 0 0 0;
  height: 85px;
  font-family: 'S-CoreDream', 'Jost', sans-serif;
  text-shadow: 0 0 1px #fff, 0 1px 2px rgba(0, 0, 0, 0.3);
}

.pan-info p {
  color: #fff;
  padding: 0.625rem 0.3125rem;
  font-style: italic;
  margin: 0 1.875rem;
  font-size: 0.95rem;
  border-top: 1px solid rgba(255, 255, 255, 0.5);
}

.pan-info p a {
  display: block;
  color: #333;
  width: 80px;
  height: 80px;
  background: rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  color: #fff;
  font-style: normal;
  font-weight: 700;
  text-transform: uppercase;
  font-size: 0.825rem;
  letter-spacing: 1px;
  padding-top: 1.5rem;
  margin: 0.4375rem auto 0;
  font-family: 'S-CoreDream', 'Jost', sans-serif;
  opacity: 0;
  transition: transform 0.3s ease-in-out 0.2s, opacity 0.3s ease-in-out 0.2s, background 0.2s linear 0s;
  transform: translateX(60px) rotate(90deg);
}

.pan-info p a:hover {
  background: rgba(255, 255, 255, 0.5);
}

.pan-item:hover .pan-thumb {
  transform: rotate(-110deg);
}

.pan-item:hover .pan-info p a {
  opacity: 1;
  transform: translateX(0px) rotate(0deg);
}
</style>

