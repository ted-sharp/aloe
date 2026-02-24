window.loginApi = {
    login: async function (loginRequest) {
        try {
            const response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                credentials: 'include', // クッキーを含める
                body: JSON.stringify(loginRequest)
            });

            if (response.ok) {
                return await response.json();
            } else {
                const error = await response.json();
                console.error('Login failed:', error);
                return null;
            }
        } catch (error) {
            console.error('Login error:', error);
            return null;
        }
    }
};

