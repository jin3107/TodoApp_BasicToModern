import dayjs, { Dayjs } from 'dayjs';

interface DateFormatterProps {
  date?: string | Dayjs;
  withTime?: boolean;
  format?: string;
}

/**
 * Component format ngày tháng theo định dạng chuẩn
 * @param date - Ngày cần format (string hoặc Dayjs)
 * @param withTime - Có hiển thị giờ phút không (default: false)
 * @param format - Custom format (override withTime)
 * @returns Chuỗi ngày đã format hoặc "-" nếu không có giá trị
 * 
 * @example
 * <DateFormatter date="2024-01-17" /> // "17/01/2024"
 * <DateFormatter date="2024-01-17T10:30:00" withTime /> // "17/01/2024 10:30"
 * <DateFormatter date="2024-01-17" format="YYYY-MM-DD" /> // "2024-01-17"
 */
export const DateFormatter = ({ date, withTime = false, format }: DateFormatterProps) => {
  if (!date) return <>-</>;
  
  const dayjsDate = typeof date === 'string' ? dayjs(date) : date;
  
  if (!dayjsDate || !dayjsDate.isValid()) return <>-</>;
  
  const defaultFormat = withTime ? 'DD/MM/YYYY HH:mm' : 'DD/MM/YYYY';
  const finalFormat = format || defaultFormat;
  
  return <>{dayjsDate.format(finalFormat)}</>;
};

/**
 * Hook utility để format date trong logic
 */
export const useDateFormatter = () => {
  const formatDate = (date?: string | Dayjs, withTime = false, customFormat?: string) => {
    if (!date) return '-';
    
    const dayjsDate = typeof date === 'string' ? dayjs(date) : date;
    
    if (!dayjsDate || !dayjsDate.isValid()) return '-';
    
    const defaultFormat = withTime ? 'DD/MM/YYYY HH:mm' : 'DD/MM/YYYY';
    const finalFormat = customFormat || defaultFormat;
    
    return dayjsDate.format(finalFormat);
  };
  
  return { formatDate };
};
