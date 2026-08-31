import { Component } from '@angular/core';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzTagModule } from 'ng-zorro-antd/tag';

@Component({
  selector: 'app-foundation-page',
  imports: [NzCardModule, NzTagModule],
  templateUrl: './foundation-page.html',
  styleUrl: './foundation-page.scss',
})
export class FoundationPage {}
