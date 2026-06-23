import React from 'react';
import { Card, Row, Col, Button } from 'antd';
import {
    UserAddOutlined,
    PlusSquareOutlined,
    AuditOutlined,
    ExportOutlined
} from '@ant-design/icons';

const QuickActionsCard: React.FC = () => {
    return (
        <Card
            title="Thao tác nhanh"
            style={{ borderRadius: 12 }}
        >
            <Row gutter={[16, 16]}>
                <Col xs={24} sm={12} md={6}>
                    <Button
                        type="primary"
                        icon={<UserAddOutlined />}
                        block
                        style={{ height: 48, borderRadius: 8, display: 'flex', alignItems: 'center', justifyContent: 'center' }}
                    >
                        Thêm nhân viên
                    </Button>
                </Col>
                <Col xs={24} sm={12} md={6}>
                    <Button
                        icon={<PlusSquareOutlined />}
                        block
                        style={{ height: 48, borderRadius: 8, display: 'flex', alignItems: 'center', justifyContent: 'center' }}
                    >
                        Tạo phòng ban
                    </Button>
                </Col>
                <Col xs={24} sm={12} md={6}>
                    <Button
                        icon={<AuditOutlined />}
                        block
                        style={{ height: 48, borderRadius: 8, display: 'flex', alignItems: 'center', justifyContent: 'center' }}
                    >
                        Duyệt đơn
                    </Button>
                </Col>
                <Col xs={24} sm={12} md={6}>
                    <Button
                        icon={<ExportOutlined />}
                        block
                        style={{ height: 48, borderRadius: 8, display: 'flex', alignItems: 'center', justifyContent: 'center' }}
                    >
                        Xuất báo cáo
                    </Button>
                </Col>
            </Row>
        </Card>
    );
};

export default QuickActionsCard;
