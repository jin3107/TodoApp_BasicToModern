import { theme, type ThemeConfig } from "antd";

/**
 * Ported 1:1 from the Classical design system handoff:
 * _ds/classical-.../styles.css (`:root`) + the `[data-theme="dark"]`
 * override block in TodoApp.dc.html's <style>.
 */
export const classicalTokens = {
  light: {
    bg: "#f3f2f2",
    surface: "#eae9e9",
    text: "#201f1d",
    accent: "#b68235",
    accentHover: "#e1ad66",
    accentActive: "#7d5411",
    divider: "rgba(32, 31, 29, 0.16)",
    neutral100: "#f8f4f4",
    neutral200: "#eae7e7",
    accent100: "#fff3e4",
    accent800: "#5a3b0a",
    shadowSm: "0 1px 2px rgba(45, 43, 43, 0.14)",
    shadowMd: "0 3px 10px rgba(45, 43, 43, 0.16)",
    shadowLg: "0 12px 32px rgba(45, 43, 43, 0.22)",
  },
  dark: {
    bg: "#1a1817",
    surface: "#242220",
    text: "#f2f0ee",
    accent: "#dbaf70",
    accentHover: "#f3dfb8",
    accentActive: "#9b7232",
    divider: "rgba(242, 240, 238, 0.16)",
    neutral100: "color-mix(in srgb, #ffffff 10%, #242220)",
    neutral200: "color-mix(in srgb, #ffffff 16%, #242220)",
    accent100: "color-mix(in srgb, #dbaf70 16%, #242220)",
    accent800: "#f3dfb8",
    shadowSm: "0 1px 2px rgba(0, 0, 0, 0.5)",
    shadowMd: "0 3px 10px rgba(0, 0, 0, 0.55)",
    shadowLg: "0 14px 34px rgba(0, 0, 0, 0.6)",
  },
} as const;

const fontHeading = '"Cormorant Garamond", system-ui, sans-serif';
const fontBody = '"Lora", system-ui, sans-serif';

export const getClassicalTheme = (isDark: boolean): ThemeConfig => {
  const t = isDark ? classicalTokens.dark : classicalTokens.light;

  return {
    algorithm: isDark ? theme.darkAlgorithm : theme.defaultAlgorithm,
    token: {
      colorPrimary: t.accent,
      colorPrimaryHover: t.accentHover,
      colorPrimaryActive: t.accentActive,
      colorBgContainer: t.surface,
      colorBgElevated: t.surface,
      colorBgLayout: t.bg,
      colorText: t.text,
      colorBorder: t.divider,
      colorBorderSecondary: t.divider,
      colorSplit: t.divider,
      borderRadius: 4,
      borderRadiusSM: 2,
      borderRadiusLG: 7,
      fontFamily: fontBody,
      fontFamilyCode: fontBody,
      boxShadow: t.shadowMd,
      boxShadowSecondary: t.shadowSm,
    },
    components: {
      Button: { borderRadius: 4, fontWeight: 600, fontFamily: fontHeading },
      Input: { borderRadius: 4, colorBgContainer: "transparent" },
      Select: { borderRadius: 4 },
      DatePicker: { borderRadius: 4 },
      Card: { borderRadiusLG: 4, colorBgContainer: "transparent" },
      Table: {
        borderRadiusLG: 7,
        headerBg: t.surface,
        headerColor: t.text,
        borderColor: t.divider,
      },
      Tag: { borderRadiusSM: 3 },
      Modal: { borderRadiusLG: 7 },
      Popover: { borderRadiusLG: 4, boxShadowSecondary: t.shadowMd },
      Notification: { borderRadiusLG: 4 },
    },
  };
};
