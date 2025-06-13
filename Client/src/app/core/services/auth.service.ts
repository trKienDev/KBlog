import { Injectable } from '@angular/core';

@Injectable({
      providedIn: 'root'
})
export class AuthService {
      constructor() { }

      // Lưu token vào localStorage khi người dùng đăng nhập
      saveToken(token: string): void {
            localStorage.setItem('authToken', token);
      }

      // Lấy token từ localStorage
      getToken(): string | null {
            return localStorage.getItem('authToken');
      }

      // Kiểm tra xem người dùng đã đăng nhập chưa (bằng cách kiểm tra sự tồn tại của token)
      isAuthenticated(): boolean {
            const token = this.getToken();
            // Một logic đầy đủ hơn có thể kiểm tra cả thời gian hết hạn của token
            return !!token;
      }

      // Lấy vai trò của người dùng từ JWT payload
      getUserRole(): string | null {
      const token = this.getToken();
      if (!token) {
            return null;
      }

      try {
            // Decode phần payload của JWT (phần thứ hai)
            const payload = JSON.parse(atob(token.split('.')[1]));
            // Giả sử vai trò được lưu trong claim có tên là 'role'
            // Tên claim này phải khớp với những gì backend trả về trong JWT
            return payload.role;
      } catch (e) {
            console.error('Error decoding JWT', e);
            return null;
      }
      }

      // Xóa token khi người dùng đăng xuất
      logout(): void {
            localStorage.removeItem('authToken');
      }
}
