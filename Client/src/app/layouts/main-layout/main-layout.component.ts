import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { HeaderComponent } from '../../layout/header/header.component';
import { FooterComponent } from '../../layout/footer/footer.component';
import { LeftSidebarComponent } from '../../layout/left-sidebar/left-sidebar.component';
import { RightSidebarComponent } from '../../layout/right-sidebar/right-sidebar.component';

@Component({
      selector: 'app-main-layout',
      standalone: true,
      // thêm mảng 'imports' & khai báo các module/component đã import
      imports: [
            RouterModule, // cần cho <router-outlet>
            HeaderComponent,
            FooterComponent,
            LeftSidebarComponent,
            RightSidebarComponent,
      ],
      templateUrl: './main-layout.component.html',
      styleUrl: './main-layout.component.scss'
})
export class MainLayoutComponent {

}
