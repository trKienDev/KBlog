import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
      {
            path: 'admin',
            loadChildren: () => import('./features/admin/admin.module').then(m => m.AdminModule),
            canActivate: [authGuard] // Protect route
      },
      {
            path: 'articles',
            // Lazy loading Articles Module when user access this path
            loadChildren: () => import('./features/articles/articles.module').then(m => m.ArticlesModule)
      }, 
      {
            path: 'auth',
            loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule)
      }, 
      {
            path: 'profile',
            loadChildren: () => import('./features/user-profile/user-profile.module').then(m => m.UserProfileModule),
            canActivate: [authGuard]
      },
      {
            path: '', //  when user access homeage without path
            redirectTo: 'articles', // 
            pathMatch: 'full'
      }
];
