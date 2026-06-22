import { Input, Button } from "antd"

export default function TableToolbar() {
    return (
        <div style={{ display: "flex", gap: 10 }}>
            <Input placeholder="Search..." style={{ width: 200 }} />
            <Button type="primary">Add</Button>
        </div>
    )
}