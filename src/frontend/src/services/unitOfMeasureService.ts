import { request } from './api';

export interface UnitOfMeasureDto {
    id: string;
    code: string;
    name: string;
}

export const unitOfMeasureService = {
    getAll: () => request<Array<UnitOfMeasureDto>>('/unit-of-measures'),
};

import { useState, useEffect } from 'react';

export function useUnitOfMeasures() {
    const [units, setUnits] = useState<UnitOfMeasureDto[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        unitOfMeasureService.getAll().then(res => {
            if (res.success && res.data) {
                setUnits(res.data);
            }
            setLoading(false);
        });
    }, []);

    return { units, loading };
}
