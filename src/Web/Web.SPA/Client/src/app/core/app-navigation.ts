/**
 * Navigation item interface
 */
export interface NavigationItem {
  text: string;
  path?: string;
  icon?: string;
  items?: NavigationItem[];
  expanded?: boolean;
  requiredScopes?: string[]; // API scopes required to access this item
}

/**
 * Application navigation configuration
 * Each item can have requiredScopes to control visibility based on user permissions
 */
export const navigation: NavigationItem[] = [
  {
    text: 'Home',
    path: '/home',
    icon: 'home',
    // No required scopes - accessible to all authenticated users
  },
  {
    text: 'Projects',
    path: '/projects',
    icon: 'product',
    requiredScopes: ['api_projects']
  },
  {
    text: 'Emailer',
    path: '/emailer',
    icon: 'email',
    requiredScopes: ['api_emailer']
  },
  {
    text: 'Chat',
    path: '/chat',
    icon: 'chat',
    // No required scopes - accessible to all authenticated users
  },
  {
    text: 'Constructor',
    path: '/constructor',
    icon: 'toolbox',
    requiredScopes: ['api_constructor']
  },
  // {
  //   text: 'Examples',
  //   icon: 'folder',
  //   items: [
  //     {
  //       text: 'Contacts',
  //       path: '/contacts',
  //       requiredScopes: ['api_contacts']
  //     }
  //   ]
  // },
  // {
  //   text: "Chart constructor",
  //   icon: "product",
  //   path: "/ctor"
  // },
  // {
  //   text: "Scheduler",
  //   icon: "event",
  //   path: "/scheduler"
  // }
];
