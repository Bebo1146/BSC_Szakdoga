import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { ProductComponent } from './product/product.component';
import { HomeComponent } from './home/home.component';
import { AuthCallbackComponent } from './auth-callback/auth-callback.component';
import { MyBidsComponent } from './my-bids/my-bids.component';
import { PaymentComponent } from './payments/payment.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: '', component: ProductComponent },
  { path: 'home', component: HomeComponent },
  { path: 'auth-callback', component: AuthCallbackComponent },
  { path: 'my-bids', component: MyBidsComponent },
  { path: 'payments', component: PaymentComponent },
  // optionally a redirect
  // { path: '', redirectTo: '/home', pathMatch: 'full' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
