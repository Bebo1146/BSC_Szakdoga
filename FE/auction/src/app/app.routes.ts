import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { ProductComponent } from './product/product.component';
import { HomeComponent } from './home/home.component';
import { AuthCallbackComponent } from './auth-callback/auth-callback.component';
import { MyBidsComponent } from './my-bids/my-bids.component';
import { PaymentComponent } from './payments/payment.component';
import { FeedbackComponent } from './feedback/feedback.component';
import { FeedbacksComponent } from './feedbacks/feedbacks.component';
import { AdminProductsComponent } from './admin-products/admin-products.component';
import { adminDomainGuard } from './guards/admin-domain.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: '', component: ProductComponent },
  { path: 'home', component: HomeComponent },
  { path: 'auth-callback', component: AuthCallbackComponent },
  { path: 'my-bids', component: MyBidsComponent },
  { path: 'payments', component: PaymentComponent },
  { path: 'feedback', component: FeedbackComponent },
  { path: 'feedbacks', component: FeedbacksComponent },
  {
    path: 'admin',
    component: AdminProductsComponent,
    canActivate: [adminDomainGuard]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
