interface ChecklistLogoProps {
  size?: number;
  color?: string;
}

/**
 * The exact "list-checks" mark used in the Classical design handoff's brand
 * lockup (header + auth shell) — CheckSquareOutlined is a different glyph,
 * so it's inlined here to match pixel-for-pixel.
 */
export const ChecklistLogo = ({ size = 22, color = "var(--color-accent)" }: ChecklistLogoProps) => (
  <svg
    width={size}
    height={size}
    viewBox="0 0 24 24"
    fill="none"
    stroke={color}
    strokeWidth={2}
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <path d="m3 17 2 2 4-4" />
    <path d="m3 7 2 2 4-4" />
    <path d="M13 6h8M13 12h8M13 18h8" />
  </svg>
);
