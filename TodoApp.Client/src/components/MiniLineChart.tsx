interface MiniLineChartPoint {
  label: string;
  value: number;
}

interface MiniLineChartProps {
  data: MiniLineChartPoint[];
  height?: number;
}

export const MiniLineChart = ({ data, height = 150 }: MiniLineChartProps) => {
  if (data.length === 0) return null;

  const max = Math.max(...data.map((d) => d.value), 1);
  const stepX = data.length > 1 ? 300 / (data.length - 1) : 0;
  const points = data.map((d, i) => ({
    x: data.length > 1 ? i * stepX : 150,
    y: 108 - (d.value / max) * 96,
  }));
  const polylinePoints = points.map((p) => `${p.x},${p.y}`).join(" ");

  return (
    <div>
      <svg viewBox="0 0 300 120" style={{ width: "100%", height }}>
        <polyline
          points={polylinePoints}
          fill="none"
          style={{ stroke: "var(--color-accent)", strokeWidth: 2 }}
        />
        {points.map((p, i) => (
          <circle
            key={i}
            cx={p.x}
            cy={p.y}
            r={2.5}
            style={{ fill: "var(--color-surface)", stroke: "var(--color-accent)", strokeWidth: 2 }}
          />
        ))}
      </svg>
      <div style={{ display: "flex", justifyContent: "space-between", fontSize: 11 }} className="text-muted">
        {data.map((d, i) => (
          <span key={i}>{d.label}</span>
        ))}
      </div>
    </div>
  );
};
