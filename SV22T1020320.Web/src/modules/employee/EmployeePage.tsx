import React from 'react';
import { Typography, Table, Tag, Space, Button, Card } from 'antd';
import { UserPlus, Mail, Phone, MapPin } from 'lucide-react';

const { Title, Text } = Typography;

const EmployeePage: React.FC = () => {
  // Placeholder data
  const data = [
    {
      key: '1',
      name: 'John Doe',
      role: 'Software Engineer',
      email: 'john.doe@novastaff.com',
      status: 'Active',
    },
    {
      key: '2',
      name: 'Jane Smith',
      role: 'Product Designer',
      email: 'jane.smith@novastaff.com',
      status: 'Active',
    },
  ];

  const columns = [
    { title: 'Name', dataIndex: 'name', key: 'name', render: (text: string) => <Text strong>{text}</Text> },
    { title: 'Role', dataIndex: 'role', key: 'role' },
    { title: 'Email', dataIndex: 'email', key: 'email' },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => (
        <Tag color="green" style={{ borderRadius: 6 }}>{status}</Tag>
      )
    },
    {
      title: 'Action',
      key: 'action',
      render: () => (
        <Space size="middle">
          <Button type="link" size="small">Edit</Button>
          <Button type="link" danger size="small">Delete</Button>
        </Space>
      ),
    },
  ];

  return (
    <div style={{ animation: 'fadeIn 0.5s ease-out' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
        <div>
          <Title level={4} style={{ margin: 0 }}>Employee Directory</Title>
          <Text type="secondary">Manage your organization's workforce and their roles.</Text>
        </div>
        <Button type="primary" icon={<UserPlus size={16} />} style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          Add Employee
        </Button>
      </div>

      <Card bordered={false} className="premium-card">
        <Table
          columns={columns}
          dataSource={data}
          pagination={false}
          style={{ borderRadius: 12, overflow: 'hidden' }}
        />
      </Card>
    </div>
  );
};

export default EmployeePage;
