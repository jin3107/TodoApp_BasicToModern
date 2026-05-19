import dayjs from 'dayjs';
import type { Dayjs } from 'dayjs';

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
