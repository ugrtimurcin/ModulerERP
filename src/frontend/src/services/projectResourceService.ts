import { request } from './api';
import type { ProjectResourceDto, CreateProjectResourceDto } from '@/types/project';

export const projectResourceService = {
    getByProject: (projectId: string) => request<ProjectResourceDto[]>(`/project-resources/project/${projectId}`),
    create: (data: CreateProjectResourceDto) => request<ProjectResourceDto>('/project-resources', { // Fixed endpoint to match controller
        method: 'POST',
        body: JSON.stringify(data)
    }),
    delete: (id: string) => request<void>(`/project-resources/${id}`, {
        method: 'DELETE'
    })
};
