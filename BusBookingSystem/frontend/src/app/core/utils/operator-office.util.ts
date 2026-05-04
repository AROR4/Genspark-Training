import { OperatorOfficeResponse } from '../api';

export function normalizeOperatorOffices(offices: OperatorOfficeResponse[] | null | undefined): OperatorOfficeResponse[] {
  return (offices ?? []).filter(office => !!office && (!!office.cityName || !!office.address));
}

export function formatOperatorOffices(offices: OperatorOfficeResponse[] | null | undefined): string {
  const normalized = normalizeOperatorOffices(offices);

  if (!normalized.length) {
    return 'No offices available';
  }

  return normalized
    .map(office => [office.cityName, office.address].filter(Boolean).join(' - '))
    .join(', ');
}
