const AuthLayout = ({ children }) => (
    <div className="min-h-screen flex items-center justify-center bg-gray-100">
        <div className="bg-white p-8 rounded-xl shadow-2xl w-full max-w-md">
            {children}
        </div>
    </div>
);
export default AuthLayout;