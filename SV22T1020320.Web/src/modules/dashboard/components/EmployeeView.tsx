import React from 'react';
import { Row, Col, Card, Timeline, Calendar, Typography, Button, Statistic, Divider, Select } from 'antd';
import { LoginOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';

const { Title, Text } = Typography;

const EmployeeView: React.FC = () => {
    const navigate = useNavigate();

    const customHeaderRender = ({ value, onChange }: any) => {
        const year = value.year();
        const month = value.month();
        const options = [];
        for (let i = year - 10; i < year + 10; i += 1) {
            options.push(<Select.Option key={i} value={i}>{i}</Select.Option>);
        }

        const months = [];
        const localeData = value.localeData();
        for (let i = 0; i < 12; i++) {
            months.push(localeData.monthsShort(value.clone().month(i)));
        }

        return (
            <div style={{ padding: 8, display: 'flex', gap: '8px', justifyContent: 'flex-end' }}>
                <Select
                    size="small"
                    // ✅ Đổi từ dropdownMatchSelectWidth -> popupMatchSelectWidth
                    popupMatchSelectWidth={false}
                    value={year}
                    onChange={(newYear) => onChange(value.clone().year(newYear))}
                >
                    {options}
                </Select>
                <Select
                    size="small"
                    popupMatchSelectWidth={false}
                    value={month}
                    onChange={(newMonth) => onChange(value.clone().month(newMonth))}
                >
                    {months.map((m, index) => (
                        <Select.Option key={index} value={index}>{m}</Select.Option>
                    ))}
                </Select>
            </div>
        );
    };

    return (
        <div className="employee-dashboard">
            <Row gutter={[24, 24]}>
                <Col xs={24} lg={16}>
                    <Card
                        variant="borderless"
                        style={{
                            background: 'linear-gradient(135deg, #1677ff 0%, #0958d9 100%)',
                            borderRadius: 16,
                            boxShadow: '0 10px 20px rgba(22,119,255,0.2)'
                        }}
                    >
                        <Row align="middle" gutter={[16, 16]}>
                            <Col flex="auto">
                                <Title level={2} style={{ color: '#fff', margin: 0 }}>Chào buổi sáng! 👋</Title>
                                <Text style={{ color: 'rgba(255,255,255,0.85)', fontSize: '16px' }}>
                                    Bạn có <b style={{ color: '#fff' }}>5 tasks</b> cần hoàn thành hôm nay.
                                </Text>
                            </Col>
                            <Col>
                                <Button
                                    size="large"
                                    shape="round"
                                    icon={<LoginOutlined />}
                                    onClick={() => navigate('/attendance')}
                                    style={{ fontWeight: 600, height: '45px' }}
                                >
                                    Check-in
                                </Button>
                            </Col>
                        </Row>
                    </Card>

                    <Card
                        title={<span style={{ fontSize: 18, fontWeight: 600 }}>Tiến độ công việc</span>}
                        style={{ marginTop: 24, borderRadius: 12 }}
                        variant="borderless"
                    >
                        <Statistic
                            title="Trạng thái hoàn thành"
                            value={18}
                            styles={{ content: { color: '#52c41a', fontWeight: '800' } }}
                            suffix="/ 23 tasks"
                        />
                        <Divider style={{ margin: '20px 0' }} />

                        <Timeline
                            // ✅ Đổi từ left -> start
                            mode="start"
                            items={[
                                {
                                    title: '09:00', // ✅ Đổi từ label -> title
                                    content: 'Họp Daily Scrum cùng Team Mobile', // ✅ Đổi từ children -> content
                                    color: 'blue'
                                },
                                {
                                    title: '14:00',
                                    content: 'Review code Pull Request #124',
                                    color: 'orange'
                                },
                                {
                                    title: '16:30',
                                    content: 'Cập nhật tiến độ trên Jira',
                                    color: 'green'
                                },
                            ]}
                        />
                    </Card>
                </Col>

                <Col xs={24} lg={8}>
                    <Card
                        title={<span style={{ fontSize: 16, fontWeight: 600 }}>Lịch làm việc</span>}
                        variant="borderless"
                        style={{ borderRadius: 12, boxShadow: '0 2px 8px rgba(0,0,0,0.04)' }}
                        styles={{ body: { padding: 8 } }}
                    >
                        <Calendar fullscreen={false} headerRender={customHeaderRender} />
                    </Card>
                </Col>
            </Row>
        </div>
    );
};

export default EmployeeView;