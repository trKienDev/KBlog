import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminDashboardComponent } from './admin-dashboard/admin-dashboard.component';
import { ManageArticlesComponent } from './manage-articles/manage-articles.component';
import { ManageCommentsComponent } from './manage-comments/manage-comments.component';
import { ManageUsersComponent } from './manage-users/manage-users.component';

const routes: Routes = [
      { 
            path: 'dashboard',
            component: AdminDashboardComponent,
      }, 
      {
            path: 'articles',
            component: ManageArticlesComponent
      }, 
      {
            path: 'users',
            component: ManageUsersComponent
      },
      {
            path: 'comments',
            component: ManageCommentsComponent
      }, 
      {
            path: '', // Default when access admin --> redirect to dashboard
            redirectTo: 'dashboard',
            pathMatch: 'full',
      }
];

@NgModule({
      imports: [RouterModule.forChild(routes)],
      exports: [RouterModule]
})
export class AdminRoutingModule { }
