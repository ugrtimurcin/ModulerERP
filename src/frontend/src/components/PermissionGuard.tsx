import React from 'react';
import { useAuthStore } from '../stores';

interface PermissionGuardProps {
    permission: string;
    children: React.ReactNode;
    fallback?: React.ReactNode;
}

export const PermissionGuard: React.FC<PermissionGuardProps> = ({ permission, children, fallback = null }) => {
    const { user } = useAuthStore();

    if (!user || !user.permissions?.includes(permission)) {
        return <>{fallback}</>;
    }

    return <>{children}</>;
};
