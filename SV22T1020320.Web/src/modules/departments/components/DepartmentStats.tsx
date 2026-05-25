import { Row, Col, Card, Typography, Skeleton } from "antd";
import { motion, AnimatePresence } from "framer-motion";

const { Title, Text } = Typography;

interface Props {
    totalDepts: number;
    totalMembers: number;
    loading?: boolean;
}

const StatValue = ({ value, loading, color }: { value: number | string, loading: boolean, color?: string }) => (
    <div style={{ height: 38, display: 'flex', alignItems: 'center' }}>
        <AnimatePresence mode="wait">
            {loading ? (
                <motion.div
                    key="loading"
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    exit={{ opacity: 0 }}
                    style={{ width: '40%' }}
                >
                    <Skeleton.Button active size="small" block style={{ height: 24, borderRadius: 4 }} />
                </motion.div>
            ) : (
                <motion.div
                    key="value"
                    initial={{ opacity: 0, y: 10, filter: 'blur(4px)' }}
                    animate={{ opacity: 1, y: 0, filter: 'blur(0px)' }}
                    transition={{ duration: 0.4, ease: [0.16, 1, 0.3, 1] }}
                >
                    <Title level={2} style={{ margin: 0, color: color }}>{value}</Title>
                </motion.div>
            )}
        </AnimatePresence>
    </div>
);

export const DepartmentStats = ({ totalDepts, totalMembers, loading = false }: Props) => (
    <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={8}>
            <Card variant="borderless" hoverable style={{ borderRadius: 8 }}>
                <div style={{ display: 'flex', flexDirection: 'column' }}>
                    <Text type="secondary">Total Departments</Text>
                    <StatValue value={totalDepts} loading={loading} />
                </div>
            </Card>
        </Col>
        <Col span={8}>
            <Card variant="borderless" hoverable style={{ borderRadius: 8 }}>
                <div style={{ display: 'flex', flexDirection: 'column' }}>
                    <Text type="secondary">Total Employees</Text>
                    <StatValue value={totalMembers} loading={loading} />
                </div>
            </Card>
        </Col>
        <Col span={8}>
            <Card variant="borderless" hoverable style={{ borderRadius: 8 }}>
                <div style={{ display: 'flex', flexDirection: 'column' }}>
                    <Text type="secondary">System Integrity</Text>
                    <StatValue value="100%" loading={loading} color="#52c41a" />
                </div>
            </Card>
        </Col>
    </Row>
);