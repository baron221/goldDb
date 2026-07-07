<template>
<el-tooltip :content="tooltipContent" placement="bottom" effect="dark">
    <div class="user-avatar-container" :style="containerStyle">
      <img v-if="avatarUrl" :src="avatarUrl" class="user-avatar-img" @error="handleImageError">
      <div v-else class="user-avatar-initials" :style="initialsStyle">
        {{ initials }}
      </div>
    </div>
  </el-tooltip>
</template>

<script>
import { defineComponent, computed, ref, watch } from 'vue';

export default defineComponent({
  name: 'UserAvatar',
  props: {
    name: {
      type: String,
      default: ''
    },
    companyName: {
      type: String,
      default: ''
    },
    avatar: {
      type: String,
      default: ''
    },
    size: {
      type: [Number, String],
      default: 30
    },
    fontSize: {
      type: Number,
      default: 0
    }
  },
  setup(props) {
    const imageError = ref(false);

    watch(() => props.avatar, () => {
      imageError.value = false;
    });

    const avatarUrl = computed(() => {
      if (imageError.value || !props.avatar) return null;

      if (props.avatar.includes('http') && !props.avatar.includes('imageView2')) {
        return `${props.avatar}?imageView2/1/w/80/h/80`;
      }
      return props.avatar;
    });

    const handleImageError = () => {
      imageError.value = true;
    };

    const initials = computed(() => {
      if (!props.name) return '?';

      const trimmedName = props.name.trim();
      if (!trimmedName) return '?';

      if (/^[a-zA-Z]/.test(trimmedName)) {
        if (trimmedName.length >= 2) {
          return trimmedName[0].toUpperCase() + trimmedName[1].toLowerCase();
        }
        return trimmedName[0].toUpperCase();
      }

      return trimmedName[0];
    });

    const containerStyle = computed(() => {
      const s = typeof props.size === 'number' ? `${props.size}px` : props.size;
      return {
        width: s,
        height: s,
        lineHeight: s
      };
    });

    const initialsStyle = computed(() => {

      let hash = 0;
      for (let i = 0; i < props.name.length; i++) {
        hash = props.name.charCodeAt(i) + ((hash << 5) - hash);
      }
      const c = (hash & 0x00FFFFFF).toString(16).toUpperCase();
      const backgroundColor = '#' + '00000'.substring(0, 6 - c.length) + c;

      const fSize = props.fontSize || (typeof props.size === 'number' ? props.size / 2.2 : 14);

      return {
        backgroundColor,
        fontSize: `${fSize}px`
      };
    });

    const tooltipContent = computed(() => {
      return props.companyName ? `${props.name} (${props.companyName})` : props.name;
    });

    return {
      avatarUrl,
      initials,
      containerStyle,
      initialsStyle,
      tooltipContent,
      handleImageError
    };
  }
});
</script>

<style lang="scss" scoped>
.user-avatar-container {
  display: inline-block;
  vertical-align: middle;
  border-radius: 50%;
  overflow: hidden;
  user-select: none;
}

.user-avatar-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.user-avatar-initials {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #ffffff;
  font-weight: bold;
  text-transform: uppercase;
}
</style>

