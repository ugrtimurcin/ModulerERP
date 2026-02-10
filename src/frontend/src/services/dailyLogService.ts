import { request } from './api';
import type { DailyLogDto, CreateDailyLogDto } from '@/types/project';

export const dailyLogService = {
    getByProject: (projectId: string) => request<DailyLogDto[]>(`/daily-log/project/${projectId}`),
    getById: (id: string) => request<DailyLogDto>(`/daily-log/${id}`),
    create: (data: CreateDailyLogDto) => request<DailyLogDto>('/daily-log', {
        method: 'POST',
        body: JSON.stringify(data)
    }),
    approve: (id: string) => request<void>(`/daily-log/${id}/approve`, {
        method: 'POST'
    })
};
