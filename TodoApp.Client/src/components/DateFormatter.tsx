import dayjs from 'dayjs';
import type { Dayjs } from 'dayjs';

interface DateFormatterProps {
  date?: string | Dayjs;
  withTime?: boolean;
  format?: string;
}

export const DateFormatter = ({ date, withTime = false, format }: DateFormatterProps) => {
  if (!date) return <>-</>;
  
  const dayjsDate = typeof date === 'string' ? dayjs(date) : date;
  
  if (!dayjsDate || !dayjsDate.isValid()) return <>-</>;
  
  const defaultFormat = withTime ? 'DD/MM/YYYY HH:mm' : 'DD/MM/YYYY';
  const finalFormat = format || defaultFormat;
  
  return <>{dayjsDate.format(finalFormat)}</>;
};
