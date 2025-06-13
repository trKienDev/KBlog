import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProfileDetailsComponent } from './profile-details/profile-details.component';
import { SavedArticlesComponent } from './saved-articles/saved-articles.component';

const routes: Routes = [
      {
            // Khi người dùng truy cập /profile --> hiển thị trang thông tin chi tiết
            path: '',
            component: ProfileDetailsComponent
      },
      {
            // Khi người dùng tủy cập profile/saved --> hiển thị trang các bài viết đã lưu
            path: 'saved',
            component: SavedArticlesComponent,
      }
];

@NgModule({
      imports: [RouterModule.forChild(routes)],
      exports: [RouterModule]
})
export class UserProfileRoutingModule { }
