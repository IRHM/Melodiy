import AuthTest from '@/app/(site)/components/AuthTest';
import React from 'react';

export default async function Home() {
  return (
    <div className="w-full h-full overflow-hidden overflow-y-auto rounded-lg">
      Home Page
      <AuthTest />
    </div>
  );
}
