import { CanActivateFn, Router } from '@angular/router';
import { Inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
      // Dùng inject() để lấy instance của AuthService & Route
      const authService = Inject(AuthService);
      const router = Inject(Router);

      // 1. Kiểm tra người dùng đã đăng nhập chưa ?
      if(!authService.isAuthenticated()) {
            // Nếu chưa --> điều hướng đến trang đăng nhập & chặn truy cập
            router.navigate(['/auth/login']);
            return false;
      }

      // 2. Nếu đã đăng nhập, kiểm tra người dùng có vai trò "Admin" không
      const userRole = authService.getUserRole();
      if(userRole === 'Admin') {
            // Nếu là Admin, cho phép truy cập
            return true;
      } else {
            // Nếu ko phải là Admin --> điều hướng về trang chủ và chặn truy cập
            // (Hoặc có thể điều hướng đến 1 trang báo lỗi 'Ko có quyền truy cập')
            alert("You do not have permission to access this page");
            router.navigate(['/articles']);
            return false;
      }

};
