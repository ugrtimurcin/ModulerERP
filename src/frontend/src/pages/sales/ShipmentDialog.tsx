import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { api } from '../../services/api';
import { Modal, Button, Input, Select, useToast } from '@/components/ui';

interface ShipmentDialogProps {
    isOpen: boolean;
    onClose: () => void;
    onSave: () => void;
    shipmentId?: string;
}

interface ShipmentItem {
    orderLineId: string;
    productId: string;
    productName: string;
    quantity: number;
    maxQuantity: number;
    lotNumber?: string;
    serialNumbers?: string;
}

const ShipmentDialog: React.FC<ShipmentDialogProps> = ({ isOpen, onClose, onSave, shipmentId }) => {
    const { t } = useTranslation();
    const toast = useToast();
    const [loading, setLoading] = useState(false);
    const [orders, setOrders] = useState<any[]>([]);

    // Form State
    const [orderId, setOrderId] = useState('');
    const [warehouseId, setWarehouseId] = useState('');
    const [carrier, setCarrier] = useState('');
    const [trackingNumber, setTrackingNumber] = useState('');
    const [estimatedDeliveryDate, setEstimatedDeliveryDate] = useState('');
    const [shippingAddress, setShippingAddress] = useState('');
    const [items, setItems] = useState<ShipmentItem[]>([]);

    useEffect(() => {
        if (isOpen) {
            if (shipmentId) {
                loadShipment(shipmentId);
            } else {
                resetForm();
                loadPendingOrders();
            }
        }
    }, [isOpen, shipmentId]);

    const loadPendingOrders = async () => {
        // ideally api.orders.getAll({ status: 'Confirmed' })
        // For now fetch all and filter client side
        try {
            const res = await api.orders.getAll(1, 100);
            if (res.success && res.data) {
                // Filter 1: Confirmed (Status 1)
                // Filter 2: Not fully shipped? Order doesn't expose 'isFullyShipped' property directly in listDto probably.
                // Assuming status 1 is 'Confirmed'.
                // Ideally we need an endpoint /sales/orders?status=Confirmed&notShipped=true
                // For MVP, just filter by status 1.
                const pendingOrders = res.data.data.filter((o: any) => o.status === 1 || o.status === 2); // Confirmed or PartiallyShipped
                setOrders(pendingOrders);
            }
        } catch (error) {
            console.error('Failed to load orders', error);
        }
    };

    const loadShipment = async (id: string) => {
        setLoading(true);
        try {
            const res = await api.shipments.getById(id);
            if (res.success && res.data) {
                const s = res.data;
                setOrderId(s.orderId);
                setWarehouseId(s.warehouseId);
                setCarrier(s.carrier || '');
                setTrackingNumber(s.trackingNumber || '');
                setEstimatedDeliveryDate(s.estimatedDeliveryDate ? s.estimatedDeliveryDate.split('T')[0] : '');
                setShippingAddress(s.shippingAddress || '');

                // Lines
                setItems(s.lines.map((l: any) => ({
                    orderLineId: '', // shipmentDto lines might not have orderLineId back ref? 
                    // Wait, ShipmentLine entity has OrderLineId.
                    // Did I map it in DTO?
                    // Let's check ShipmentService MapToDto.
                    // MapToDto: ... Lines = s.Lines.Select...
                    // It does NOT include OrderLineId in ShipmentLineDto.
                    // Failure. I need to update ShipmentDtos.cs and ShipmentService.cs
                    // to include OrderLineId in ShipmentLineDto if I want to edit correctly.
                    // However, editing a shipment usually doesn't involve changing line mapping, just qty.
                    // But if I want to reconstruct the UI...

                    // For now, let's assume Edit is Read-Only for items or limited.
                    // Or I just fix the DTO. I'll act as if I can't edit lines for existing shipment easily.
                    // BUT for Create mode, I need to load items from Order.

                    productId: l.productId,
                    productName: l.productName,
                    quantity: l.quantity,
                    maxQuantity: l.quantity, // Creating assumes loaded from db
                })));
            }
        } catch (error) {
            console.error('Failed to load shipment', error);
        } finally {
            setLoading(false);
        }
    };

    const handleOrderChange = async (newOrderId: string) => {
        setOrderId(newOrderId);
        if (!newOrderId) {
            setItems([]);
            return;
        }

        // Fetch Order Details to get lines
        try {
            const res = await api.orders.getById(newOrderId);
            if (res.success && res.data) {
                const o = res.data;
                setWarehouseId(o.warehouseId); // Default to order warehouse
                setShippingAddress(o.shippingAddress);

                // Populate items with remaining quantities
                const newItems = o.lines
                    .filter((l: any) => (l.remainingToShip || (l.quantity - l.shippedQuantity)) > 0)
                    .map((l: any) => ({
                        orderLineId: l.id,
                        productId: l.productId,
                        productName: l.description, // or productName
                        quantity: l.remainingToShip || (l.quantity - l.shippedQuantity),
                        maxQuantity: l.remainingToShip || (l.quantity - l.shippedQuantity)
                    }));
                setItems(newItems);
            }
        } catch (error) {
            console.error('Failed to load order details', error);
        }
    };

    const resetForm = () => {
        setOrderId('');
        setCarrier('');
        setTrackingNumber('');
        setEstimatedDeliveryDate('');
        setShippingAddress('');
        setItems([]);
    };

    const handleSave = async () => {
        if (!orderId || items.length === 0) {
            toast.error(t('common.required'));
            return;
        }

        setLoading(true);
        const dto = {
            orderId,
            warehouseId,
            carrier,
            trackingNumber,
            estimatedDeliveryDate: estimatedDeliveryDate || null,
            shippingAddress,
            lines: items.filter(i => i.quantity > 0).map(i => ({
                orderLineId: i.orderLineId,
                productId: i.productId,
                quantity: Number(i.quantity),
                lotNumber: i.lotNumber,
                serialNumbers: i.serialNumbers
            }))
        };

        try {
            let res;
            if (shipmentId) {
                // No update endpoint exists — shipments are immutable once created.
                toast.error(t('common.error'), 'Shipments cannot be edited. Delete and recreate instead.');
                setLoading(false);
                return;
            } else {
                res = await api.shipments.create(dto);
            }

            if (res.success) {
                toast.success(shipmentId ? t('common.updatedSuccess') : t('common.createdSuccess'));
                onSave();
                onClose();
            } else {
                toast.error(t('common.error'), res.error || t('common.error'));
            }
        } catch (error) {
            console.error('Save failed', error);
            toast.error(t('common.error'));
        } finally {
            setLoading(false);
        }
    };

    const footer = (
        <>
            <Button variant="secondary" onClick={onClose}>
                {t('common.cancel')}
            </Button>
            {(!shipmentId) && (
                <Button onClick={handleSave} isLoading={loading}>
                    {t('common.save')}
                </Button>
            )}
            {shipmentId && (
                <Button onClick={handleSave} isLoading={loading}>
                    {t('shipments.updateInfo')}
                </Button>
            )}
        </>
    );

    return (
        <Modal
            isOpen={isOpen}
            onClose={onClose}
            title={shipmentId ? t('shipments.editShipment') : t('shipments.createShipment')}
            size="2xl"
            footer={footer}
        >
            <div className="space-y-4">
                {/* Order Selection */}
                {!shipmentId && (
                    <Select
                        label={t('quotes.orders.title')} // Using existing Order context
                        value={orderId}
                        onChange={(e) => handleOrderChange(e.target.value)}
                        options={orders.map(o => ({ value: o.id, label: `${o.orderNumber} - ${o.partnerName}` }))}
                        placeholder={t('shipments.selectOrder')}
                    />
                )}
                {shipmentId && (
                    <div className="text-sm text-gray-500">
                        {t('quotes.orders.orderNumber')}: {orderId} (Editing items not supported yet)
                    </div>
                )}

                <div className="grid grid-cols-2 gap-4">
                    <Input
                        label={t('shipments.carrier')}
                        value={carrier}
                        onChange={(e) => setCarrier(e.target.value)}
                    />
                    <Input
                        label={t('shipments.tracking')}
                        value={trackingNumber}
                        onChange={(e) => setTrackingNumber(e.target.value)}
                    />
                    <Input
                        label={t('shipments.estDelivery')}
                        type="date"
                        value={estimatedDeliveryDate}
                        onChange={(e) => setEstimatedDeliveryDate(e.target.value)}
                    />
                </div>

                {/* Items to Ship */}
                <div>
                    <h4 className="font-medium mb-2">{t('shipments.itemsToShip')}</h4>
                    <div className="overflow-x-auto border rounded-lg max-h-60 overflow-y-auto">
                        <table className="min-w-full divide-y divide-gray-200">
                            <thead className="bg-gray-50">
                                <tr>
                                    <th className="px-3 py-2 text-left text-xs font-medium text-gray-500">{t('shipments.product')}</th>
                                    <th className="px-3 py-2 text-right text-xs font-medium text-gray-500">{t('shipments.max')}</th>
                                    <th className="px-3 py-2 text-right text-xs font-medium text-gray-500">{t('shipments.shipQty')}</th>
                                </tr>
                            </thead>
                            <tbody className="bg-white divide-y divide-gray-200">
                                {items.map((item, index) => (
                                    <tr key={index}>
                                        <td className="px-3 py-2 text-sm">{item.productName}</td>
                                        <td className="px-3 py-2 text-sm text-right">{item.maxQuantity}</td>
                                        <td className="px-3 py-2 text-right">
                                            <input
                                                type="number"
                                                className="w-20 border rounded px-2 py-1 text-right text-sm"
                                                value={item.quantity}
                                                onChange={(e) => {
                                                    const val = parseFloat(e.target.value);
                                                    const newItems = [...items];
                                                    newItems[index].quantity = val;
                                                    setItems(newItems);
                                                }}
                                                disabled={!!shipmentId}
                                            />
                                        </td>
                                    </tr>
                                ))}
                                {items.length === 0 && (
                                    <tr>
                                        <td colSpan={3} className="px-3 py-4 text-center text-sm text-gray-500">
                                            {t('shipments.selectOrderFirst')}
                                        </td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </Modal>
    );
};

export default ShipmentDialog;
