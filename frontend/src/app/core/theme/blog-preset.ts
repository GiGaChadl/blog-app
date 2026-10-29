import { definePreset } from '@primeng/themes';
import Aura from '@primeng/themes/aura';

/**
 * BlogPreset — theme "Sổ tay cá nhân"
 * Ánh xạ token thiết kế của blog (xem src/styles.scss) vào hệ thống theme PrimeNG.
 * KHÔNG dùng màu mặc định của Aura — mọi giá trị bên dưới lấy từ bảng màu đã chốt:
 *   Paper #F5F1E8 · Ink #22282E · Amber #B9702E · Sage #56685A · Margin #9B3B3B
 */
export const BlogPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '#FBF3EA',
      100: '#F3DEC3',
      200: '#E9C79A',
      300: '#DBA968',
      400: '#CB8B45',
      500: '#B9702E', // Amber — màu hành động chính
      600: '#9C5D25',
      700: '#7E4A1E',
      800: '#603817',
      900: '#442810',
      950: '#2B1909',
    },
    colorScheme: {
      light: {
        surface: {
          0: '#FFFFFF',
          50: '#F5F1E8', // Paper
          100: '#EFE9DA',
          200: '#DDD6C6', // Line (hairline)
          300: '#C7BEA8',
          400: '#A79C82',
          500: '#8A7E64',
          600: '#6B6250',
          700: '#4A5158',
          800: '#33393E',
          900: '#22282E', // Ink
          950: '#171B1F',
        },
        primary: {
          color: '#B9702E',
          contrastColor: '#FFFFFF',
          hoverColor: '#9C5D25',
          activeColor: '#7E4A1E',
        },
        text: {
          color: '#22282E',
          hoverColor: '#171B1F',
          mutedColor: '#4A5158',
          hoverMutedColor: '#33393E',
        },
        content: {
          background: '#FFFFFF',
          hoverBackground: '#F5F1E8',
          borderColor: '#DDD6C6',
        },
        highlight: {
          background: '#FBF3EA',
          focusBackground: '#F3DEC3',
          color: '#7E4A1E',
          focusColor: '#603817',
        },
      },
      dark: {
        surface: {
          0: '#242A30',
          50: '#1B1F24',
          100: '#242A30',
          200: '#363C42',
          300: '#4A5158',
          400: '#5E6167',
          500: '#8A8578',
          600: '#ABA69B',
          700: '#C7C2B4',
          800: '#E4E0D4',
          900: '#F1EDE3',
          950: '#FAF8F3',
        },
        primary: {
          color: '#E0A165',
          contrastColor: '#1B1F24',
          hoverColor: '#F0BE8C',
          activeColor: '#F0BE8C',
        },
        text: {
          color: '#F1EDE3',
          hoverColor: '#FAF8F3',
          mutedColor: '#C7C2B4',
          hoverMutedColor: '#E4E0D4',
        },
        content: {
          background: '#242A30',
          hoverBackground: '#2C333A',
          borderColor: '#363C42',
        },
        highlight: {
          background: '#3A2A1B',
          focusBackground: '#4A3624',
          color: '#F0BE8C',
          focusColor: '#F5D4AC',
        },
      },
    },
  },
  components: {
    button: {
      root: {
        borderRadius: '8px',
      },
    },
    card: {
      root: {
        borderRadius: '10px',
      },
    },
    inputtext: {
      root: {
        borderRadius: '8px',
      },
    },
    multiselect: {
      option: {
        color: '{content.hoverBackground}',
        focusColor: '{text.color}',
        focusBackground: '{content.hoverBackground}',
        selectedColor: '{primary.color}',
        selectedBackground: '{highlight.background}',
      },
    },
  },
});
