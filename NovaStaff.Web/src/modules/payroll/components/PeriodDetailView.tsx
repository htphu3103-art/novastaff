import React, { useState, useEffect, useCallback } from 'react';
import { Card, Button, Typography, Space, message, Row, Col, Spin, Popconfirm, Tag, Statistic, Divider, Modal, Input } from 'antd';
import { CloseOutlined, CalculatorOutlined, CheckCircleOutlined, SendOutlined, InfoCircleOutlined, DownloadOutlined, SafetyCertificateOutlined } from '@ant-design/icons';
import { payrollApi } from '../api/payrollApi';
import { PayrollPeriodDetailDto, PayrollDetailDto, PayrollStatus } from '../types';
import { PayrollTable } from './PayrollTable';
import { PayslipModal } from './PayslipModal';
import { AdjustmentModal } from './AdjustmentModal';
import dayjs from 'dayjs';

const { Title, Text } = Typography;

interface PeriodDetailViewProps {
    periodId: number;
    onBack: () => void;
}

const getStatusTag = (status: PayrollStatus) => {
    switch (status) {
        case PayrollStatus.Draft: return <Tag color="default">Nháp</Tag>;
        case PayrollStatus.Calculated: return <Tag color="blue">Đã tính</Tag>;
        case PayrollStatus.Approved: return <Tag color="green">Đã duyệt</Tag>;
        case PayrollStatus.Paid: return <Tag color="gold">Đã thanh toán</Tag>;
        default: return <Tag>{status}</Tag>;
    }
};

export const PeriodDetailView: React.FC<PeriodDetailViewProps> = ({ periodId, onBack }) => {
    const [periodDetail, setPeriodDetail] = useState<PayrollPeriodDetailDto | null>(null);
    const [loading, setLoading] = useState(false);
    const [batchLoading, setBatchLoading] = useState(false);
    const [statusLoading, setStatusLoading] = useState(false);
    const [adjustLoading, setAdjustLoading] = useState(false);

    // Thống kê state
    const [totalNetSalary, setTotalNetSalary] = useState<number>(0);
    const [paidCount, setPaidCount] = useState<number>(0);

    // For Payslip Modal
    const [isPayslipOpen, setIsPayslipOpen] = useState(false);
    const [selectedDetail, setSelectedDetail] = useState<PayrollDetailDto | null>(null);

    // For Adjustment Modal
    const [isAdjustOpen, setIsAdjustOpen] = useState(false);

    // OTP Modal state
    const [isSecurityModalOpen, setIsSecurityModalOpen] = useState(false);
    const [otpCode, setOtpCode] = useState('');

    const loadDetail = useCallback(async () => {
        setLoading(true);
        try {
            const [resDetail, resTotal, resSummary] = await Promise.all([
                payrollApi.getPeriodDetail(periodId),
                payrollApi.getTotalNetSalary(periodId).catch(() => ({ data: { totalNetSalary: 0 } })),
                payrollApi.getStatusSummary(periodId).catch(() => ({ data: [] }))
            ]);
            
            setPeriodDetail(resDetail.data);
            setTotalNetSalary(resTotal.data.totalNetSalary || 0);

            const summaryData = resSummary.data || [];
            let paidEmployees = 0;
            if (Array.isArray(summaryData)) {
                const paidItem = summaryData.find((x: any) => x.status === PayrollStatus.Paid);
                paidEmployees = paidItem ? paidItem.count : 0;
            } else if (summaryData && typeof summaryData === 'object') {
                paidEmployees = summaryData[PayrollStatus.Paid] || summaryData['Paid'] || summaryData['4'] || 0;
            }
            setPaidCount(paidEmployees);

        } catch (error: any) {
            console.error('Lỗi khi tải chi tiết kỳ lương:', error);
            message.error('Không thể tải chi tiết kỳ lương');
        } finally {
            setLoading(false);
        }
    }, [periodId]);

    useEffect(() => {
        loadDetail();
    }, [loadDetail]);

    const handleBatchCalculate = async () => {
        setBatchLoading(true);
        try {
            const res = await payrollApi.batchCalculate(periodId, { periodID: periodId, departmentID: null });
            message.success(`Tính lương thành công cho ${res.data.successCount} nhân viên. Bỏ qua: ${res.data.skippedCount}`);
            if (res.data.errors && res.data.errors.length > 0) {
                console.warn('Errors during batch calculate:', res.data.errors);
            }
            await loadDetail();
        } catch (error: any) {
            console.error('Lỗi khi tính lương hàng loạt:', error);
            const errMsg = error.response?.data?.detail || error.response?.data?.message || (typeof error.response?.data === 'string' ? error.response.data : 'Có lỗi xảy ra khi tính lương');
            message.error(errMsg);
        } finally {
            setBatchLoading(false);
        }
    };

    const handleAdvanceStatus = async (targetStatus: PayrollStatus) => {
        setStatusLoading(true);
        try {
            await payrollApi.advancePeriodStatus(periodId, { periodID: periodId, targetStatus });
            message.success('Cập nhật trạng thái kỳ lương thành công');
            await loadDetail();
        } catch (error: any) {
            console.error('Lỗi khi cập nhật trạng thái:', error);
            const errMsg = error.response?.data?.detail || error.response?.data?.message || (typeof error.response?.data === 'string' ? error.response.data : 'Có lỗi xảy ra khi cập nhật trạng thái');
            message.error(errMsg);
        } finally {
            setStatusLoading(false);
        }
    };

    const handleViewPayslip = (record: PayrollDetailDto) => {
        setSelectedDetail(record);
        setIsPayslipOpen(true);
    };

    const handleOpenAdjust = (record: PayrollDetailDto) => {
        setSelectedDetail(record);
        setIsAdjustOpen(true);
    };

    const handleSaveAdjustments = async (values: any) => {
        // Kiểm tra cả 2 trường hợp ID để chắc chắn (do có thể backend trả về camelCase)
        const empId = selectedDetail?.employeeID || (selectedDetail as any)?.employeeId;
        
        if (!empId) {
            message.error('Không tìm thấy ID nhân viên để thực hiện điều chỉnh!');
            return;
        }

        setAdjustLoading(true);
        try {
            await payrollApi.updateAdjustments(periodId, empId, values);
            message.success(`Đã cập nhật điều chỉnh lương cho ${selectedDetail?.fullName}`);
            setIsAdjustOpen(false);
            await loadDetail();
        } catch (error: any) {
            console.error('Lỗi khi cập nhật điều chỉnh:', error);
            const errMsg = error.response?.data?.detail || error.response?.data?.message || 'Không thể lưu các khoản điều chỉnh';
            message.error(errMsg);
        } finally {
            setAdjustLoading(false);
        }
    };

    const handleRecalculate = async (record: PayrollDetailDto) => {
        if (!record.employeeID) return;
        setLoading(true);
        try {
            await payrollApi.calculatePayslip(periodId, record.employeeID);
            message.success(`Đã tính lại lương cho nhân viên ${record.fullName}`);
            await loadDetail();
        } catch (error: any) {
            console.error('Lỗi khi tính lại lương:', error);
            message.error('Có lỗi xảy ra khi tính lại lương');
        } finally {
            setLoading(false);
        }
    };

    const handleExportCSV = () => {
        if (!periodDetail || !periodDetail.details) return;
        
        // Tạo nội dung CSV
        const headers = ['Mã NV', 'Họ tên', 'Phòng ban', 'Lương cơ bản', 'Thực lĩnh', 'Số tài khoản'];
        const rows = periodDetail.details.map(d => [
            d.employeeCode,
            d.fullName,
            d.departmentName || '',
            d.baseSalarySnapshot,
            d.netSalary,
            '123456789' // Dummy STK
        ]);
        
        const csvContent = [
            headers.join(';'),
            ...rows.map(r => r.join(';'))
        ].join('\n');
        
        // Thêm BOM để Excel đọc được tiếng Việt
        const bom = new Uint8Array([0xEF, 0xBB, 0xBF]);
        const blob = new Blob([bom, csvContent], { type: 'text/csv;charset=utf-8;' });
        
        // Tạo thẻ a ẩn để download
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);
        link.setAttribute('href', url);
        link.setAttribute('download', `Bang_Luong_Thang_${periodDetail.month}_${periodDetail.year}.csv`);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    };

    const handleConfirmPayment = () => {
        if (otpCode === '123456') {
            setIsSecurityModalOpen(false);
            handleAdvanceStatus(PayrollStatus.Paid);
            setOtpCode('');
        } else {
            message.error('Mã OTP không hợp lệ! Vui lòng nhập 123456');
        }
    };

    if (loading && !periodDetail) {
        return (
            <div style={{ textAlign: 'center', padding: '50px' }}>
                <Spin size="large" />
                <div style={{ marginTop: 16 }}>Đang tải chi tiết kỳ lương...</div>
            </div>
        );
    }

    const totalEmployees = periodDetail?.details?.length || 0;

    return (
        <Card 
            {...({ variant: 'borderless' } as any)}
            {...({ styles: { body: { padding: 0 } } } as any)}
            style={{ 
                borderRadius: '8px', 
                overflow: 'hidden',
                boxShadow: '0 4px 20px rgba(0, 0, 0, 0.08)',
                border: '1px solid #d9d9d9'
            }}
        >
            {/* 1. Header Khu Vực Chi Tiết */}
            <div style={{ backgroundColor: '#e6f4ff', padding: '20px 24px', borderBottom: '1px solid #bae0ff' }}>
                <Row justify="space-between" align="middle">
                    <Col>
                        <Space size="large" align="center">
                            <Button shape="circle" icon={<CloseOutlined />} onClick={onBack} title="Đóng chi tiết" />
                            <div>
                                <Space align="center" style={{ marginBottom: 4 }}>
                                    <Title level={4} style={{ margin: 0, color: '#003eb3' }}>
                                        Kỳ Lương Tháng {periodDetail?.month}/{periodDetail?.year}
                                    </Title>
                                    {periodDetail && getStatusTag(periodDetail.status)}
                                </Space>
                                <div style={{ color: '#0958d9', fontSize: '13px' }}>
                                    <InfoCircleOutlined style={{ marginRight: 6 }} />
                                    Chu kỳ chốt công: {periodDetail?.startDate ? dayjs(periodDetail.startDate).format('DD/MM/YYYY') : ''} - {periodDetail?.endDate ? dayjs(periodDetail.endDate).format('DD/MM/YYYY') : ''}
                                </div>
                            </div>
                        </Space>
                    </Col>
                    <Col>
                        {/* Thanh công cụ hành động */}
                        <Space size="middle">
                            {(!periodDetail || periodDetail.status < PayrollStatus.Approved) && (
                                <Popconfirm
                                    title="Xác nhận tính lương"
                                    description="Hệ thống sẽ cập nhật lại ngày công và tính lương. Chắc chắn tiếp tục?"
                                    onConfirm={handleBatchCalculate}
                                    okText="Đồng ý"
                                    cancelText="Hủy"
                                >
                                    <Button icon={<CalculatorOutlined />} loading={batchLoading}>
                                        Tính lương hàng loạt
                                    </Button>
                                </Popconfirm>
                            )}

                            {periodDetail?.status === PayrollStatus.Draft && (
                                <Popconfirm
                                    title="Hoàn tất tính toán"
                                    description="Sau khi chốt, kỳ lương sẽ chuyển sang trạng thái chờ Duyệt. Bạn chắc chắn chứ?"
                                    onConfirm={() => handleAdvanceStatus(PayrollStatus.Calculated)}
                                    okText="Chốt ngay"
                                    cancelText="Hủy"
                                >
                                    <Button type="primary" loading={statusLoading}>
                                        Hoàn tất tính toán
                                    </Button>
                                </Popconfirm>
                            )}

                            {periodDetail?.status === PayrollStatus.Calculated && (
                                <Popconfirm
                                    title="Duyệt kỳ lương"
                                    description="Sau khi duyệt, bảng lương bị khóa và không thể tính lại. Xác nhận Duyệt?"
                                    onConfirm={() => handleAdvanceStatus(PayrollStatus.Approved)}
                                    okText="Duyệt"
                                    cancelText="Hủy"
                                >
                                    <Button type="primary" icon={<CheckCircleOutlined />} loading={statusLoading}>
                                        Duyệt kỳ lương
                                    </Button>
                                </Popconfirm>
                            )}

                            {periodDetail?.status === PayrollStatus.Approved && (
                                <>
                                    <Button icon={<DownloadOutlined />} onClick={handleExportCSV}>
                                        Xuất file (CSV)
                                    </Button>
                                    <Button type="primary" icon={<SendOutlined />} style={{ backgroundColor: '#52c41a' }} onClick={() => setIsSecurityModalOpen(true)}>
                                        Đánh dấu Đã Thanh Toán
                                    </Button>
                                </>
                            )}
                        </Space>
                    </Col>
                </Row>
            </div>

            {/* 2. Thanh Tóm tắt (Mini Stats) cho riêng tháng này */}
            <div style={{ backgroundColor: '#fafafa', padding: '16px 24px', borderBottom: '1px solid #f0f0f0' }}>
                <Row gutter={32}>
                    <Col>
                        <Statistic title="Tổng số nhân sự" value={totalEmployees} suffix="người" {...({ styles: { content: { fontSize: '20px' } } } as any)} />
                    </Col>
                    <Col>
                        <Divider {...({ vertical: true } as any)} style={{ height: '100%' }} />
                    </Col>
                    <Col>
                        <Statistic title="Tổng quỹ lương (Tháng này)" value={totalNetSalary} suffix="đ" {...({ styles: { content: { fontSize: '20px', color: '#cf1322' } } } as any)} />
                    </Col>
                    <Col>
                        <Divider {...({ vertical: true } as any)} style={{ height: '100%' }} />
                    </Col>
                    <Col>
                        <Statistic title="Tiến độ chi trả" value={paidCount} suffix={`/ ${totalEmployees} người`} {...({ styles: { content: { fontSize: '20px', color: paidCount === totalEmployees && totalEmployees > 0 ? '#389e0d' : '#000' } } } as any)} />
                    </Col>
                </Row>
            </div>

            {/* 3. Bảng Dữ Liệu */}
            <div style={{ padding: '24px' }}>
                <PayrollTable 
                    dataSource={periodDetail?.details || []} 
                    loading={loading}
                    onViewPayslip={handleViewPayslip}
                    onRecalculate={handleRecalculate}
                    onAdjust={handleOpenAdjust}
                />
            </div>

            <PayslipModal 
                open={isPayslipOpen}
                data={selectedDetail}
                onCancel={() => setIsPayslipOpen(false)}
                periodInfo={periodDetail ? { month: periodDetail.month, year: periodDetail.year } : undefined}
            />

            <AdjustmentModal
                open={isAdjustOpen}
                data={selectedDetail}
                onCancel={() => setIsAdjustOpen(false)}
                onSave={handleSaveAdjustments}
                loading={adjustLoading}
            />

            <Modal
                title={
                    <Space>
                        <SafetyCertificateOutlined style={{ color: '#52c41a' }} />
                        <span>Xác thực giao dịch thanh toán</span>
                    </Space>
                }
                open={isSecurityModalOpen}
                onCancel={() => {
                    setIsSecurityModalOpen(false);
                    setOtpCode('');
                }}
                onOk={handleConfirmPayment}
                okText="Xác nhận giao dịch"
                cancelText="Hủy"
                okButtonProps={{ loading: statusLoading, style: { backgroundColor: '#52c41a' } }}
            >
                <div style={{ padding: '16px 0' }}>
                    <div style={{ marginBottom: 16, padding: '12px', backgroundColor: '#f6ffed', border: '1px solid #b7eb8f', borderRadius: 8 }}>
                        <Text strong>Tóm tắt giao dịch:</Text>
                        <ul style={{ margin: '8px 0 0 0', paddingLeft: 20 }}>
                            <li>Tổng số tiền: <b style={{ color: '#cf1322' }}>{totalNetSalary.toLocaleString()}đ</b></li>
                            <li>Số lượng nhân sự: <b>{totalEmployees}</b> người</li>
                        </ul>
                    </div>
                    <Typography.Paragraph>
                        Hệ thống đã gửi mã OTP đến số điện thoại của Giám Đốc. Vui lòng nhập mã để xác nhận thao tác cập nhật trạng thái bảng lương. (Mã test: <b>123456</b>)
                    </Typography.Paragraph>
                    <Input 
                        size="large" 
                        placeholder="Nhập mã OTP 6 số" 
                        maxLength={6}
                        value={otpCode}
                        onChange={(e) => setOtpCode(e.target.value)}
                        style={{ textAlign: 'center', letterSpacing: '8px', fontSize: '20px' }}
                    />
                </div>
            </Modal>
        </Card>
    );
};
