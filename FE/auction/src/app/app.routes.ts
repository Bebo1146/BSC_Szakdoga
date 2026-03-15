import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { ProductComponent } from './product/product.component';
import { HomeComponent } from './home/home.component';
import { AuthCallbackComponent } from './auth-callback/auth-callback.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: '', component: ProductComponent },
  { path: 'home', component: HomeComponent },
  { path: 'auth-callback', component: AuthCallbackComponent }
];
