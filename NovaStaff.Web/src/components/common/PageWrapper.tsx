import { Breadcrumb } from "antd"

export default function PageWrapper({ title, children }: any) {
    return (
        <div>
            <h2 style={{ marginBottom: 8 }}>{title}</h2>

            <Breadcrumb
                items={[
                    { title: "Home" },
                    { title }
                ]}
            />

            <div style={{ marginTop: 16 }}>{children}</div>
        </div>
    )
}