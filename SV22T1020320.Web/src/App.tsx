import { BrowserRouter } from "react-router-dom"
import AppRoutes from "./routes"
import { AuthProvider } from "./contexts/AuthContext"
import { GlobalAuthInit } from "./components/GlobalAuthInit"
import { App as AntApp } from "antd"

function App() {
  return (
    <BrowserRouter>
      <AntApp>
        <AuthProvider>
          <GlobalAuthInit>
            <AppRoutes />
          </GlobalAuthInit>
        </AuthProvider>
      </AntApp>
    </BrowserRouter>
  )
}

export default App