Describe 'DummyTest' -Tag 'DummyTest' {
    It 'Dummy Test' {
        $dummy = 'This is a dummy test'
        $dummy | Should -Not -BeNullOrEmpty
        $dummy | Should -Be 'This is a dummy test'

    }
}