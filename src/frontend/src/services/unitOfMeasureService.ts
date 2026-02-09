import { request } from './api';

export interface UnitOfMeasureDto {
    id: string;
    code: string;
    name: string;
}

export const unitOfMeasureService = {
    getAll: () => request<Array<UnitOfMeasureDto>>('/unit-of-measures'),
};
